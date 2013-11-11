﻿using System.Collections.Generic;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using umbraco.cms.businesslogic;
using System;
using System.Xml;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_Relation_Tests : BaseDatabaseFactoryTest
    {
        #region Helper methods
        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        private void l(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        private bool _traceTestCompletion = false;
        private int _testNumber;
        private void traceCompletion(string finished = "Finished")
        {
            if (!_traceTestCompletion) return; 
            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            string message = string.Format("***** {0:000}. {1} - {2} *****\n", ++_testNumber, methodBase.Name, finished);
            System.Console.Write(message);
        }
        #endregion

        #region EnsureData()
        public override void Initialize()
        {
            base.Initialize();
            EnsureData(); 
        }

        private bool initialized;
        private UmbracoContext context;
        private UmbracoDatabase database;

        private CMSNode _node1;
        private CMSNode _node2;
        private CMSNode _node3;
        private CMSNode _node4;
        private CMSNode _node5;

        private RelationType _relationType1;
        private RelationType _relationType2;
        private Relation _relation1;
        private Relation _relation2;
        private Relation _relation3;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void EnsureData()
        {
            if (!initialized)
            {               
                MakeNew_PersistsNewUmbracoNodeRow();

                _relationType1 = insertTestRelationType(1);
                _relationType2 = insertTestRelationType(2); 

                _relation1 = insertTestRelation(_relationType1,  _node1.Id, _node2.Id, "node1(parent) <-> node2(child)");
                _relation2 = insertTestRelation(_relationType1,  _node2.Id, _node3.Id, "node2(parent) <-> node3(child)");
                _relation3 = insertTestRelation(_relationType2,  _node1.Id, _node3.Id, "node1(parent) <-> node3(child)");

            }

            initialized = true;
        }

        private const string TEST_RELATION_TYPE_NAME = "Test Relation Type";
        private const string TEST_RELATION_TYPE_ALIAS = "testRelationTypeAlias";
        private RelationType insertTestRelationType(int testRelationTypeNumber)
        {
            database.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                            "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                            new { dual = 1, parentObjectType = Guid.NewGuid(), childObjectType = Guid.NewGuid(),
                                  name = string.Format("{0}_{1}", TEST_RELATION_TYPE_NAME, testRelationTypeNumber),
                                  alias = string.Format("{0}_{1}", TEST_RELATION_TYPE_ALIAS, testRelationTypeNumber),
                            });
            int relationTypeId = database.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
            return new RelationType(relationTypeId);
        }

        private Relation insertTestRelation(RelationType relationType, int parentNodeId, int childNodeId, string comment)
        {
            database.Execute("insert into [umbracoRelation] (parentId, childId, relType, datetime, comment) values (@parentId, @childId, @relType, @datetime, @comment)",
                             new { parentId = parentNodeId, childId = childNodeId, relType = relationType.Id, datetime = DateTime.Now, comment = comment });
            int relationId = database.ExecuteScalar<int>("select max(id) from [umbracoRelation]");
            return new Relation(relationId);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            // nodes deletion recursively deletes all relations...
           if (_node1 != null)  _node1.delete();
           if (_node2 != null)  _node2.delete();
           if (_node3 != null) _node3.delete();
           if (_node4 != null) _node4.delete();
           if (_node5 != null) _node5.delete();

           //... bit not relation types
           if (_relationType1 != null) database.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType1.Id);
           if (_relationType2 != null) database.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType2.Id);

           initialized = false; 
        }

        private void CreateContext()
        {
            context = GetUmbracoContext("http://localhost", 0);
            database = context.Application.DatabaseContext.Database;
        }
 
        #endregion

        #region Tests
        [Test(Description = "Verify if EnsureData() and related helper test methods execute well")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);

            //int count1 = database.ExecuteScalar<int>("select Count(*) from umbracoNode");
            //l("Count = {0}", count1); // = 31 + 3

            //int count2 = database.ExecuteScalar<int>("select count(*) from [umbracoRelationType]");
            //l("Count = {0}", count2); // = 1 + 1

            //int count3 = database.ExecuteScalar<int>("select count(*) from [umbracoRelation]");
            //l("Count = {0}", count3); // = 0 + 3

            Assert.That(_node1, !Is.Null);   
            Assert.That(CMSNode.IsNode(_node1.Id), Is.True);
            Assert.That(_node2, !Is.Null);
            Assert.That(CMSNode.IsNode(_node2.Id), Is.True);
            Assert.That(_node3, !Is.Null);
            Assert.That(CMSNode.IsNode(_node3.Id), Is.True);
            Assert.That(_node4, !Is.Null);
            Assert.That(CMSNode.IsNode(_node4.Id), Is.True);
            Assert.That(_node5, !Is.Null);
            Assert.That(CMSNode.IsNode(_node5.Id), Is.True);

            Assert.That(CMSNode.CountByObjectType(Document._objectType), Is.EqualTo(5));

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);

            EnsureAllTestRecordsAreDeleted();

            //int count21 = database.ExecuteScalar<int>("select count(*) from [umbracoRelationType]");
            //l("Count = {0}", count21); // = 1 + 0

            //int count31 = database.ExecuteScalar<int>("select count(*) from [umbracoRelation]");
            //l("Count = {0}", count31); // = 0 + 0

            Assert.That(CMSNode.CountByObjectType(Document._objectType), Is.EqualTo(0));

            // see the next test code line: Assert.Throws(typeof(ArgumentException), delegate { RelationType.GetById(_relationType.Id); });
            Assert.That(RelationType.GetById(_relationType1.Id), Is.Null);
            Assert.That(RelationType.GetById(_relationType2.Id), Is.Null);
            Assert.That(Relation.GetRelations(_relation1.Id).Length, Is.EqualTo(0));
            Assert.That(Relation.GetRelations(_relation2.Id).Length, Is.EqualTo(0));
            Assert.That(Relation.GetRelations(_relation3.Id).Length, Is.EqualTo(0));


            traceCompletion();
        }

        [Test(Description = "Test 'public Relation(int Id)' constructor")]
        public void Test_Constructor()
        {
            // persisted test relation
            var testRelation = new Relation(_relation1.Id);
            Assert.That(_relation1.Id, Is.EqualTo(testRelation.Id));

            // not persisted Relation
            Assert.Throws(typeof(ArgumentException), delegate { new Relation(12345); });

            traceCompletion();
        }

        [Test(Description = "Test 'public CMSNode Parent' property.set")]
        public void Test_Parent_set()
        {
            var oldParent = _relation1.Parent;

            try
            {
                // before new parent node is set
                var testRelation1 = new Relation(_relation1.Id);
                Assert.That(testRelation1.Parent.Id, Is.EqualTo(_node1.Id));

                _relation1.Parent = _node4;

                // after new parent node is set
                var testRelation2 = new Relation(_relation1.Id);
                Assert.That(testRelation2.Parent.Id, Is.EqualTo(_node4.Id));
            }
            finally
            {
                // reset
                _relation1.Parent = oldParent;
            }
            traceCompletion();
        }

        [Test(Description = "Test 'public CMSNode Child' property.set")]
        public void Test_Child_set()
        {
            var oldChild = _relation1.Child;

            try
            {
                // before new child node is set
                var testRelation1 = new Relation(_relation1.Id);
                Assert.That(testRelation1.Child.Id, Is.EqualTo(_node2.Id));

                _relation1.Child = _node4;

                // after new child node is set
                var testRelation2 = new Relation(_relation1.Id);
                Assert.That(testRelation2.Child.Id, Is.EqualTo(_node4.Id));
            }
            finally 
            {
                // reset
                _relation1.Child = oldChild;
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public string Comment' property.set")]
        public void Test_Comment_set()
        {
            string oldComment = _relation1.Comment;
            string newComment = "my new comment";

            try
            {
                // before new comment value is set
                var testRelation1 = new Relation(_relation1.Id);
                Assert.That(testRelation1.Comment, !Is.EqualTo(newComment));

                _relation1.Comment = newComment;

                // after new comment value is set
                var testRelation2 = new Relation(_relation1.Id);
                Assert.That(testRelation2.Comment, Is.EqualTo(newComment));
            }
            finally
            {
                // reset
                _relation1.Comment = oldComment;  
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public RelationType RelType' property.set")]
        public void Test_RelationType_set()
        {
            var oldRelationType = _relation1.RelType; 

            try
            {
                // before new comment value is set
                var testRelation1 = new Relation(_relation1.Id);
                Assert.That(testRelation1.RelType.Id, Is.EqualTo(oldRelationType.Id));

                _relation1.RelType = _relationType2;

                // after new comment value is set
                var testRelation2 = new Relation(_relation1.Id);
                Assert.That(testRelation2.RelType.Id, Is.EqualTo(_relationType2.Id));
            }
            finally
            {
                // reset
                _relation1.RelType = _relationType1; 
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public Delete' method")]
        public void Test_Relation_Delete()
        {
            var relationId = _relation1.Id;

            try
            {
  
                // before relation is deleted
                var testRelation1 = new Relation(relationId);
                Assert.That(testRelation1.Id, Is.EqualTo(_relation1.Id));

                _relation1.Delete();

                // after relation is deleted
                Assert.Throws<ArgumentException>( () => { new Relation(relationId); });  
            }
            finally
            {
                // reset
                EnsureAllTestRecordsAreDeleted(); 
            }

            traceCompletion();
        }
        
        [Test(Description = "Test 'public static Relation MakeNew(int parentId, int childId, RelationType relType, string comment)' method")]
        public void Test_Relation_MakeNew()
        {
            Relation testRelation1 = null;
            Relation testRelation2 = null;

            try
            {
                testRelation1 = Relation.MakeNew(_node4.Id, _node5.Id, _relationType2, "Test Relation MakeNew");
                int testRelationId = testRelation1.Id;
                testRelation2 = new Relation(testRelationId);

                Assert.That(testRelationId, Is.EqualTo(testRelation2.Id));

                testRelation2.Delete();
                testRelation2 = null;

                // after relation is deleted
                Assert.Throws<ArgumentException>(() => { new Relation(testRelationId); });
            }
            finally
            {
                // reset
                if (testRelation2 != null) testRelation2.Delete();   
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public static Relation[] GetRelations(int NodeId)' method")]
        public void Test_Relation_GetRelations_1()
        {
            var relations = Relation.GetRelations(_node1.Id);
            // there are two test relations for _node1 created in EnsureData()
            Assert.That(relations.Length, Is.EqualTo(2));

            traceCompletion();
        }

        [Test(Description = "public static List<Relation> GetRelationsAsList(int NodeId)' method")]
        public void Test_Relation_GetRelationsAsList()
        {
            var relations = Relation.GetRelationsAsList(_node1.Id);
            // there are two test relations for _node1 created in EnsureData()
            Assert.That(relations.Count, Is.EqualTo(2));

            traceCompletion();
        }

        [Test(Description = "Test 'public static Relation[] GetRelations(int NodeId, RelationType Filter)' method")]
        public void Test_Relation_GetRelations_2()
        {
            var relations1 = Relation.GetRelations(_node1.Id, _relationType1);
            // there is one test relation of type _ralationType1 for _node1 created in EnsureData()
            Assert.That(relations1.Length, Is.EqualTo(1));

            var relations2 = Relation.GetRelations(_node1.Id, _relationType2);
            // there is one test relation of type _ralationType2 for _node1 created in EnsureData()
            Assert.That(relations1.Length, Is.EqualTo(1));

            traceCompletion();
        }

        [Test(Description = "Test 'public static bool IsRelated(int ParentID, int ChildId)' method")]
        public void Test_Relation_IsRelated_1()
        {
            var result1 = Relation.IsRelated(_node1.Id, _node2.Id);
            Assert.IsTrue(result1);
            var result2 = Relation.IsRelated(_node1.Id, _node4.Id);
            Assert.IsFalse(result2);

            traceCompletion();
        }

        [Test(Description = "Test 'public static bool IsRelated(int ParentID, int ChildId, RelationType Filter)' method")]
        public void Test_Relation_IsRelated_2()
        {
            var result1 = Relation.IsRelated(_node1.Id, _node2.Id, _relationType1);
            Assert.IsTrue(result1);
            var result2 = Relation.IsRelated(_node1.Id, _node2.Id, _relationType2);
            Assert.IsFalse(result2);

            traceCompletion();
        }

        [Test(Description = "Test 'internal PopulateFromDto(RelationDto relationDto)' method")]
        public void Test_Relation_PopulateFromDto()
        {
            var testRelation = new Relation(_relation1.Id);
            Assert.That(testRelation.Id, Is.EqualTo(_relation1.Id));    
            Assert.That(testRelation.Parent.Id, Is.EqualTo(_relation1.Parent.Id));    
            Assert.That(testRelation.Child.Id, Is.EqualTo(_relation1.Child.Id));    
            Assert.That(testRelation.RelType.Id, Is.EqualTo(_relation1.RelType.Id));    
            Assert.That(testRelation.Comment, Is.EqualTo(_relation1.Comment));    
            Assert.That(testRelation.CreateDate, Is.EqualTo(_relation1.CreateDate));    

            traceCompletion();
        }

        #endregion

        #region Private Helper classes and methods
        private class TestCMSNode : CMSNode
        {
            public TestCMSNode(int id)
                : base(id)
            {
            }

            private TestCMSNode(int id, bool nosetup)
                : base(id, nosetup)
            {
            }

            public static CMSNode MakeNew(
                int parentId,
                int level,
                string text,
                Guid objectType)
            {
                return CMSNode.MakeNew(parentId, objectType, 0, level, text, Guid.NewGuid());
            }

            public void ExecuteSavePreviewXml(XmlDocument xd, Guid versionId)
            {
                SavePreviewXml(ToXml(xd, false), versionId);
            }

            public XmlNode ExecuteGetPreviewXml(XmlDocument xd, Guid versionId)
            {
                return GetPreviewXml(xd, versionId);
            }

            public static int[] ExecuteGetUniquesFromObjectTypeAndFirstLetter(Guid objectType, char letter)
            {
                return getUniquesFromObjectTypeAndFirstLetter(objectType, letter);
            }

            public static CMSNode CreateUsingSetupNode(int id)
            {
                var node = new TestCMSNode(id, true);
                node.setupNode();
                return node;
            }
        }

        public void MakeNew_PersistsNewUmbracoNodeRow()
        {
            // Testing Document._objectType, since it has exclusive use of GetNewDocumentSortOrder. :)

            int id = 0;
            try
            {
                CreateContext();
                _node1 = TestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);
                var totalDocuments = database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @ObjectTypeId",
                    new { ObjectTypeId = Document._objectType });
                Assert.AreEqual(5, totalDocuments);
                id = _node3.Id;
                var loadedNode = new CMSNode(id);
                AssertNonEmptyNode(loadedNode);
                Assert.AreEqual(2, loadedNode.sortOrder);
            }
            finally
            {
                //DeleteContent();
            }
        }

        private void AssertNonEmptyNode(CMSNode node)
        {
            Assert.AreNotEqual(0, node.Id);
            Assert.AreNotEqual(Guid.Empty, node.UniqueId);
            Assert.AreNotEqual(Guid.Empty, node.nodeObjectType);
            Assert.IsNotNullOrEmpty(node.Path);
            Assert.AreNotEqual(0, node.ParentId);
            Assert.IsNotNullOrEmpty(node.Text);
            Assert.AreEqual(DateTime.Today, node.CreateDateTime.Date);
            Assert.IsFalse(node.IsTrashed);
        }

       #endregion
    }
}
