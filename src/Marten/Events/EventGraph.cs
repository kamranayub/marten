using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Marten.Events.Projections;
using Marten.Schema;

namespace Marten.Events
{
    public class EventGraph : IProjections
    {
        private readonly ConcurrentDictionary<string, IAggregator> _aggregateByName =
            new ConcurrentDictionary<string, IAggregator>();

        private readonly ConcurrentDictionary<Type, IAggregator> _aggregates =
            new ConcurrentDictionary<Type, IAggregator>();

        private readonly Cache<string, EventMapping> _byEventName = new Cache<string, EventMapping>();
        private readonly Cache<Type, EventMapping> _events = new Cache<Type, EventMapping>();


        private string _databaseSchemaName;

        public EventGraph(StoreOptions options)
        {
            Options = options;
            _events.OnMissing = eventType => typeof(EventMapping<>).CloseAndBuildAs<EventMapping>(this, eventType);

            _byEventName.OnMissing = name => { return AllEvents().FirstOrDefault(x => x.EventTypeName == name); };

            SchemaObjects = new EventStoreDatabaseObjects(this);
        }

        internal StoreOptions Options { get; }

        // TODO -- make this internal
        public TableName Table => new TableName(DatabaseSchemaName, "mt_streams");

        // TODO -- make this internal
        public IDocumentSchemaObjects SchemaObjects { get; }

        public bool JavascriptProjectionsEnabled { get; set; }

        public EventMapping EventMappingFor(Type eventType)
        {
            return _events[eventType];
        }

        public EventMapping EventMappingFor<T>() where T : class, new()
        {
            return EventMappingFor(typeof(T));
        }

        public IEnumerable<EventMapping> AllEvents()
        {
            return _events;
        }

        public IEnumerable<IAggregator> AllAggregates()
        {
            return _aggregates.Values;
        }

        public EventMapping EventMappingFor(string eventType)
        {
            return _byEventName[eventType];
        }

        public void AddEventType(Type eventType)
        {
            _events.FillDefault(eventType);
        }

        public void AddEventTypes(IEnumerable<Type> types)
        {
            types.Each(AddEventType);
        }


        public bool IsActive => _events.Any() || _aggregates.Any();

        public string DatabaseSchemaName
        {
            get { return _databaseSchemaName ?? Options.DatabaseSchemaName; }
            set { _databaseSchemaName = value; }
        }


        public void AggregateFor<T>(IAggregator<T> aggregator) where T : class, new()
        {
            _aggregates.AddOrUpdate(typeof(T), aggregator, (type, previous) => aggregator);
        }

        public IAggregator<T> AggregateFor<T>() where T : class, new()
        {
            return _aggregates
                .GetOrAdd(typeof(T), type => new Aggregator<T>())
                .As<IAggregator<T>>();
        }


        public Type AggregateTypeFor(string aggregateTypeName)
        {
            return
                _aggregateByName.GetOrAdd(aggregateTypeName,
                    name => { return AllAggregates().FirstOrDefault(x => x.Alias == name); }).AggregateType;
        }

        public IAggregator<T> AggregateStreamsInlineWith<T>() where T : class, new()
        {
            var aggregator = AggregateFor<T>();
            var finder = new AggregateFinder<T>();
            var projection = new AggregationProjection<T>(finder, aggregator);

            Inlines.Add(projection);

            return aggregator;
        }

        public void TransformEventsInlineWith<TEvent, TView>(ITransform<TEvent, TView> transform)
        {
            var projection = new OneForOneProjection<TEvent, TView>(transform);
            Inlines.Add(projection);
        }

        public void InlineTransformation(IProjection projection)
        {
            Inlines.Add(projection);
        }


        public IList<IProjection> Inlines { get; } = new List<IProjection>();

        public string AggregateAliasFor(Type aggregateType)
        {
            return _aggregates
                .GetOrAdd(aggregateType, type => typeof(Aggregator<>).CloseAndBuildAs<IAggregator>(aggregateType)).Alias;
        }
    }
}