﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using Marten.Schema;
using Marten.Schema.Identity;
using Marten.Schema.Identity.Sequences;
using Marten.Testing.Documents;
using Marten.Testing.Schema.Hierarchies;
using Shouldly;
using StructureMap;
using Xunit;

namespace Marten.Testing.Schema
{
    public class DocumentMappingTests : IntegratedFixture
    {
        public class FieldId
        {
            public string id;
        }

        public abstract class AbstractDoc
        {
            public int id;
        }

        public interface IDoc
        {
            string id { get; set; }
        }


        [Fact]
        public void concrete_type_with_subclasses_is_hierarchy()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.AddSubClass(typeof(SuperUser));

            mapping.IsHierarchy().ShouldBeTrue();
        }

        [Fact]
        public void default_alias_for_a_type_that_is_not_nested()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias.ShouldBe("user");
        }

        [Fact]
        public void default_table_name()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Table.Name.ShouldBe("mt_doc_user");
        }

        [Fact]
        public void default_table_name_with_different_shema()
        {
            var mapping = DocumentMapping.For<User>("other");
            mapping.Table.QualifiedName.ShouldBe("other.mt_doc_user");
        }

        [Fact]
        public void default_table_name_with_schema()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Table.QualifiedName.ShouldBe("public.mt_doc_user");
        }

        [Fact]
        public void default_upsert_name()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.UpsertFunction.Name.ShouldBe("mt_upsert_user");
        }

        [Fact]
        public void default_upsert_name_with_different_schema()
        {
            var mapping = DocumentMapping.For<User>("other");
            mapping.UpsertFunction.QualifiedName.ShouldBe("other.mt_upsert_user");
        }

        [Fact]
        public void default_upsert_name_with_schema()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.UpsertFunction.QualifiedName.ShouldBe("public.mt_upsert_user");
        }

        [Fact]
        public void get_the_sql_locator_for_the_Id_member()
        {
            DocumentMapping.For<User>().FieldFor("Id")
                .SqlLocator.ShouldBe("d.id");

            DocumentMapping.For<FieldId>().FieldFor("id")
                .SqlLocator.ShouldBe("d.id");
        }

        [Fact]
        public void is_hierarchy__is_false_for_concrete_type_with_no_subclasses()
        {
            DocumentMapping.For<User>().IsHierarchy().ShouldBeFalse();
        }

        [Fact]
        public void is_hierarchy_always_true_for_abstract_type()
        {
            DocumentMapping.For<AbstractDoc>()
                .IsHierarchy().ShouldBeTrue();
        }

        [Fact]
        public void is_hierarchy_always_true_for_interface()
        {
            DocumentMapping.For<IDoc>().IsHierarchy()
                .ShouldBeTrue();
        }

        [Fact]
        public void optimistic_versioning_is_turned_off_by_default()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.UseOptimisticConcurrency.ShouldBeFalse();
        }

        [Fact]
        public void override_the_alias()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "users";

            mapping.Table.Name.ShouldBe("mt_doc_users");
            mapping.UpsertFunction.Name.ShouldBe("mt_upsert_users");
        }

        [Fact]
        public void override_the_alias_converts_alias_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "Users";

            mapping.Alias.ShouldBe("users");
        }

        [Fact]
        public void override_the_alias_converts_table_name_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "Users";

            mapping.Table.Name.ShouldBe("mt_doc_users");
        }

        [Fact]
        public void override_the_alias_converts_tablename_with_different_schema_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>("OTHER");
            mapping.Alias = "Users";

            mapping.Table.QualifiedName.ShouldBe("other.mt_doc_users");
        }

        [Fact]
        public void override_the_alias_converts_tablename_with_schema_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "Users";

            mapping.Table.QualifiedName.ShouldBe("public.mt_doc_users");
        }

        [Fact]
        public void override_the_alias_converts_upsertname_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "Users";

            mapping.UpsertFunction.Name.ShouldBe("mt_upsert_users");
        }

        [Fact]
        public void override_the_alias_converts_upsertname_with_different_schema_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>("OTHER");
            mapping.Alias = "Users";

            mapping.UpsertFunction.QualifiedName.ShouldBe("other.mt_upsert_users");
        }

        [Fact]
        public void override_the_alias_converts_upsertname_with_schema_to_lowercase()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Alias = "Users";

            mapping.UpsertFunction.QualifiedName.ShouldBe("public.mt_upsert_users");
        }

        [Fact]
        public void select_fields_for_non_hierarchy_mapping()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.SelectFields().ShouldHaveTheSameElementsAs("data", "id", DocumentMapping.VersionColumn);
        }

        [Fact]
        public void select_fields_with_subclasses()
        {
            var mapping = DocumentMapping.For<Squad>();
            mapping.AddSubClass(typeof(BaseballTeam));

            mapping.SelectFields().ShouldHaveTheSameElementsAs("data", "id", DocumentMapping.DocumentTypeColumn, DocumentMapping.VersionColumn);
        }

        [Fact]
        public void select_fields_without_subclasses()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.SelectFields().ShouldHaveTheSameElementsAs("data", "id", DocumentMapping.VersionColumn);
        }

        [Fact]
        public void to_table_columns_with_duplicated_fields()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.DuplicateField("FirstName");
            mapping.SchemaObjects.As<DocumentSchemaObjects>().StorageTable().Columns.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("id", "data", DocumentMapping.LastModifiedColumn,
                    DocumentMapping.VersionColumn, DocumentMapping.DotNetTypeColumn, "first_name");
        }

        [Fact]
        public void to_table_columns_with_subclasses()
        {
            var mapping = DocumentMapping.For<Squad>();
            mapping.AddSubClass(typeof(BaseballTeam));

            var table = mapping.SchemaObjects.As<DocumentSchemaObjects>().StorageTable();

            var typeColumn = table.Columns.Last();
            typeColumn.Name.ShouldBe(DocumentMapping.DocumentTypeColumn);
            typeColumn.Type.ShouldBe("varchar");
        }

        [Fact]
        public void to_table_without_subclasses_and_no_duplicated_fields()
        {
            var mapping = DocumentMapping.For<IntDoc>();
            mapping.SchemaObjects.As<DocumentSchemaObjects>().StorageTable().Columns.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("id", "data", DocumentMapping.LastModifiedColumn,
                    DocumentMapping.VersionColumn, DocumentMapping.DotNetTypeColumn);
        }

        [Fact]
        public void to_upsert_baseline()
        {
            var mapping = DocumentMapping.For<Squad>();
            var function = new UpsertFunction(mapping);

            function.Arguments.Select(x => x.Column)
                .ShouldHaveTheSameElementsAs("id", "data", DocumentMapping.VersionColumn,
                    DocumentMapping.DotNetTypeColumn);
        }

        [Fact]
        public void to_upsert_with_duplicated_fields()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.DuplicateField("FirstName");
            mapping.DuplicateField("LastName");

            var function = new UpsertFunction(mapping);

            var args = function.Arguments.Select(x => x.Column).ToArray();
            args.ShouldContain("first_name");
            args.ShouldContain("last_name");
        }

        [Fact]
        public void to_upsert_with_subclasses()
        {
            var mapping = DocumentMapping.For<Squad>();
            mapping.AddSubClass(typeof(BaseballTeam));

            var function = new UpsertFunction(mapping);

            function.Arguments.Select(x => x.Column)
                .ShouldHaveTheSameElementsAs("id", "data", DocumentMapping.VersionColumn,
                    DocumentMapping.DotNetTypeColumn, DocumentMapping.DocumentTypeColumn);
        }

        [Fact]
        public void doc_type_with_use_optimistic_concurrency_attribute()
        {
            DocumentMapping.For<VersionedDoc>()
                .UseOptimisticConcurrency.ShouldBeTrue();
        }

        [UseOptimisticConcurrency]
        public class VersionedDoc
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }

        [Fact]
        public void default_table_name_2()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.Table.QualifiedName.ShouldBe("public.mt_doc_user");
        }

        [Fact]
        public void default_table_name_on_other_schema()
        {
            var mapping = DocumentMapping.For<User>("other");
            mapping.Table.QualifiedName.ShouldBe("other.mt_doc_user");
        }

        [Fact]
        public void default_table_name_on_overriden_schema()
        {
            var mapping = DocumentMapping.For<User>("other");
            mapping.DatabaseSchemaName = "overriden";
            mapping.Table.QualifiedName.ShouldBe("overriden.mt_doc_user");
        }

        [Fact]
        public void default_search_mode_is_jsonb_to_record()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.PropertySearching.ShouldBe(PropertySearching.JSON_Locator_Only);
        }

        [Fact]
        public void pick_up_upper_case_property_id()
        {
            var mapping = DocumentMapping.For<UpperCaseProperty>();
            mapping.IdMember.ShouldBeAssignableTo<PropertyInfo>()
                .Name.ShouldBe(nameof(UpperCaseProperty.Id));
        }

        [Fact]
        public void pick_up_lower_case_property_id()
        {
            var mapping = DocumentMapping.For<LowerCaseProperty>();
            mapping.IdMember.ShouldBeAssignableTo<PropertyInfo>()
                .Name.ShouldBe(nameof(LowerCaseProperty.id));
        }

        [Fact]
        public void pick_up_lower_case_field_id()
        {
            var mapping = DocumentMapping.For<LowerCaseField>();
            mapping.IdMember.ShouldBeAssignableTo<FieldInfo>()
                .Name.ShouldBe(nameof(LowerCaseField.id));
        }

        [Fact]
        public void pick_up_upper_case_field_id()
        {
            var mapping = DocumentMapping.For<UpperCaseField>();
            mapping.IdMember.ShouldBeAssignableTo<FieldInfo>()
                .Name.ShouldBe(nameof(UpperCaseField.Id));
        }

        [Fact]
        public void generate_simple_document_table()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>();
            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("CREATE TABLE public.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("jsonb NOT NULL");
        }

        [Fact]
        public void generate_simple_document_table_on_other_schema()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>("other");
            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("CREATE TABLE other.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("jsonb NOT NULL");
        }

        [Fact]
        public void generate_simple_document_table_on_overriden_schema()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>("other");
            mapping.DatabaseSchemaName = "overriden";

            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("CREATE TABLE overriden.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("jsonb NOT NULL");
        }

        [Fact]
        public void generate_table_with_indexes()
        {
            var mapping = DocumentMapping.For<User>();
            var i1 = mapping.AddIndex("first_name");
            var i2 = mapping.AddIndex("last_name");

            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain(i1.ToDDL());
            sql.ShouldContain(i2.ToDDL());
        }

        [Fact]
        public void generate_table_with_foreign_key()
        {
            var mapping = DocumentMapping.For<Issue>();
            var foreignKey = mapping.AddForeignKey("AssigneeId", typeof (User));

            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain(foreignKey.ToDDL());
        }

        [Fact]
        public void generate_a_document_table_with_duplicated_tables()
        {
            var mapping = DocumentMapping.For<User>();
            mapping.DuplicateField("FirstName");

            var table = mapping.SchemaObjects.As<DocumentSchemaObjects>().StorageTable();

            table.Columns.Any(x => x.Name == "first_name").ShouldBeTrue();
        }

        [Fact]
        public void generate_a_table_to_the_database_with_duplicated_field()
        {
            using (var container = Container.For<DevelopmentModeRegistry>())
            {
                container.GetInstance<DocumentCleaner>().CompletelyRemove(typeof (User));

                var schema = container.GetInstance<IDocumentSchema>();

                var mapping = schema.MappingFor(typeof (User)).As<DocumentMapping>();
                mapping.DuplicateField("FirstName");

                var storage = schema.StorageFor(typeof (User));

                schema.DbObjects.DocumentTables().ShouldContain(mapping.Table.QualifiedName);
            }
        }

        [Fact]
        public void write_upsert_sql()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>();
            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("INSERT INTO public.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("CREATE OR REPLACE FUNCTION public.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void write_upsert_sql_on_other_schema()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>("other");
            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("INSERT INTO other.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("CREATE OR REPLACE FUNCTION other.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void write_upsert_sql_on_overriden_schema()
        {
            var mapping = DocumentMapping.For<MySpecialDocument>("other");
            mapping.DatabaseSchemaName = "overriden";

            var builder = new StringWriter();

            mapping.SchemaObjects.WriteSchemaObjects(theStore.Schema, builder);

            var sql = builder.ToString();

            sql.ShouldContain("INSERT INTO overriden.mt_doc_documentmappingtests_myspecialdocument");
            sql.ShouldContain("CREATE OR REPLACE FUNCTION overriden.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void table_name_with_schema_for_document()
        {
            DocumentMapping.For<MySpecialDocument>().Table.QualifiedName
                .ShouldBe("public.mt_doc_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void table_name_with_schema_for_document_on_other_schema()
        {
            DocumentMapping.For<MySpecialDocument>("other").Table.QualifiedName
                .ShouldBe("other.mt_doc_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void table_name_with_schema_for_document_on_overriden_schema()
        {
            var documentMapping = DocumentMapping.For<MySpecialDocument>("other");
            documentMapping.DatabaseSchemaName = "overriden";

            documentMapping.Table.QualifiedName
                .ShouldBe("overriden.mt_doc_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void upsert_name_with_schema_for_document_type()
        {
            DocumentMapping.For<MySpecialDocument>().UpsertFunction.QualifiedName
                .ShouldBe("public.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void upsert_name_with_schema_for_document_type_on_other_schema()
        {
            DocumentMapping.For<MySpecialDocument>("other").UpsertFunction.QualifiedName
                .ShouldBe("other.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void upsert_name_with_schema_for_document_type_on_overriden_schema()
        {
            var documentMapping = DocumentMapping.For<MySpecialDocument>("other");
            documentMapping.DatabaseSchemaName = "overriden";

            documentMapping.UpsertFunction.QualifiedName
                .ShouldBe("overriden.mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void table_name_for_document()
        {
            DocumentMapping.For<MySpecialDocument>().Table.Name
                .ShouldBe("mt_doc_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void upsert_name_for_document_type()
        {
            DocumentMapping.For<MySpecialDocument>().UpsertFunction.Name
                .ShouldBe("mt_upsert_documentmappingtests_myspecialdocument");
        }

        [Fact]
        public void find_field_for_immediate_property_that_is_not_duplicated()
        {
            var mapping = DocumentMapping.For<UpperCaseProperty>();
            var field = mapping.FieldFor("Id");
            field.Members.Single().ShouldBeAssignableTo<PropertyInfo>()
                .Name.ShouldBe("Id");
        }

        [Fact]
        public void find_field_for_immediate_field_that_is_not_duplicated()
        {
            var mapping = DocumentMapping.For<UpperCaseField>();
            var field = mapping.FieldFor("Id");
            field.Members.Single().ShouldBeAssignableTo<FieldInfo>()
                .Name.ShouldBe("Id");
        }

        [Fact]
        public void duplicate_a_field()
        {
            var mapping = DocumentMapping.For<User>();

            mapping.DuplicateField("FirstName");

            mapping.FieldFor("FirstName").ShouldBeOfType<DuplicatedField>();

            // other fields are still the same

            mapping.FieldFor("LastName").ShouldNotBeOfType<DuplicatedField>();
        }

        [Fact]
        public void switch_to_only_using_json_locator_fields()
        {
            var mapping = DocumentMapping.For<User>();

            mapping.DuplicateField("FirstName");

            mapping.PropertySearching = PropertySearching.JSON_Locator_Only;

            mapping.FieldFor("LastName").ShouldBeOfType<JsonLocatorField>();

            // leave duplicates alone

            mapping.FieldFor("FirstName").ShouldBeOfType<DuplicatedField>();
        }


        [Fact]
        public void picks_up_searchable_attribute_on_fields()
        {
            var mapping = DocumentMapping.For<Organization>();

            mapping.FieldFor("OtherName").ShouldBeOfType<DuplicatedField>();
            mapping.FieldFor(nameof(Organization.OtherField)).ShouldNotBeOfType<DuplicatedField>();
        }

        [Fact]
        public void picks_up_searchable_attribute_on_properties()
        {
            var mapping = DocumentMapping.For<Organization>();

            mapping.FieldFor(nameof(Organization.Name)).ShouldBeOfType<DuplicatedField>();
            mapping.FieldFor(nameof(Organization.OtherProp)).ShouldNotBeOfType<DuplicatedField>();
        }

        [Fact]
        public void picks_up_marten_attibute_on_document_type()
        {
            var mapping = DocumentMapping.For<Organization>();
            mapping.PropertySearching.ShouldBe(PropertySearching.JSON_Locator_Only);
        }

        [Fact]
        public void use_string_id_generation_for_string()
        {
            var mapping = DocumentMapping.For<StringId>();
            mapping.IdStrategy.ShouldBeOfType<StringIdGeneration>();
        }

        [Fact]
        public void use_guid_id_generation_for_guid_id()
        {
            var mapping = DocumentMapping.For<UpperCaseProperty>();
            mapping.IdStrategy.ShouldBeOfType<CombGuidIdGeneration>();
        }

        [Fact]
        public void use_hilo_id_generation_for_int_id()
        {
            DocumentMapping.For<IntId>()
                .IdStrategy.ShouldBeOfType<HiloIdGeneration>();
        }

        [Fact]
        public void use_hilo_id_generation_for_long_id()
        {
            DocumentMapping.For<LongId>()
                .IdStrategy.ShouldBeOfType<HiloIdGeneration>();
        }

        [Fact]
        public void use_custom_default_id_generation_for_long_id()
        {
            DocumentMapping.For<LongId>(idGeneration: (m, o) => new CustomIdGeneration())
                .IdStrategy.ShouldBeOfType<CustomIdGeneration>();
        }

        [Fact]
        public void use_custom_id_generation_on_mapping_shoudl_be_settable()
        {
            var mapping = DocumentMapping.For<LongId>();

            mapping.IdStrategy = new CustomIdGeneration();
            mapping.IdStrategy.ShouldBeOfType<CustomIdGeneration>();
        }

        [Fact]
        public void can_replace_hilo_def_settings()
        {
            var mapping = DocumentMapping.For<LongId>();

            var newDef = new HiloSettings {MaxLo = 33};

            mapping.HiloSettings(newDef);

            var sequence = mapping.IdStrategy.ShouldBeOfType<HiloIdGeneration>();
            sequence.MaxLo.ShouldBe(newDef.MaxLo);

        }

        [Fact]
        public void trying_to_replace_the_hilo_settings_when_not_using_hilo_for_the_sequence_throws()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                DocumentMapping.For<StringId>().HiloSettings(new HiloSettings());
            });
        }


        public class IntId
        {
            public int Id;
        }

        public class LongId
        {
            public long Id;
        }

        public class StringId
        {
            public string Id;
        }

        public class UpperCaseProperty
        {
            public Guid Id { get; set; }
        }

        public class LowerCaseProperty
        {
            public Guid id { get; set; }
        }

        public class UpperCaseField
        {
            public int Id;
        }

        public class LowerCaseField
        {
            public int id;
        }

        public class MySpecialDocument
        {
            public Guid Id { get; set; }
        }

        [PropertySearching(PropertySearching.JSON_Locator_Only)]
        public class Organization
        {
            public Guid Id { get; set; }

            [DuplicateField]
            public string Name { get; set; }

            [DuplicateField] public string OtherName;

            public string OtherProp;
            public string OtherField { get; set; }
        }

        public class CustomIdGeneration : IIdGeneration
        {
            public IEnumerable<Type> KeyTypes { get; }

            public IIdGenerator<T> Build<T>(IDocumentSchema schema)
            {
                throw new NotImplementedException();
            }
        }
    }
}