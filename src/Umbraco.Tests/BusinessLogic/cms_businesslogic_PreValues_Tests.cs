﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core.Persistence;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_PreValues_Tests : BaseDatabaseFactoryTest
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

        private int _testNumber;
        private void traceCompletion(string done = "Done")
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            string message = string.Format("***** {0:000}. {1} - {2} *****\n", ++_testNumber, methodBase.Name, done);
            System.Console.Write(message);
        }
        #endregion

        #region EnsureData()
        private bool initialized;

        private UserType _userType;
        private User _user;
        private DataTypeDefinition _dataTypeDefinition1;
        private int _dataTypeDefinition1_PrevaluesTestCount;
        private DataTypeDefinition _dataTypeDefinition2;
        private int _dataTypeDefinition2_PrevaluesTestCount;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void EnsureData()
        {
            if (!initialized)
            {
                _userType = UserType.MakeNew("Tester", "CADMOSKTPIURZ:5F", "Test User");
                _user = User.MakeNew("TEST", "TEST", "abcdefg012345", _userType);
                _dataTypeDefinition1 = DataTypeDefinition.MakeNew(_user, "Nvarchar");
                _dataTypeDefinition2 = DataTypeDefinition.MakeNew(_user, "Ntext");
            }

            var values = new List<string> 
                {
                    "default",
                    ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|",
                    "test"
                };

            if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition1.Id) == 0)
            {
                initialized = false;

                // insert three test PreValue records
                _dataTypeDefinition1_PrevaluesTestCount = values.Count;
                values.ForEach(x =>
                    {
                        PreValue.Database.Execute(
                            "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                            new { dtdefid = _dataTypeDefinition1.Id, value = x });
                    });
            }

            if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition2.Id) == 0)
            {
                initialized = false;

                // insert one test PreValueRecord
                _dataTypeDefinition2_PrevaluesTestCount = 1;
                PreValue.Database.Execute(
                    "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                    new { dtdefid = _dataTypeDefinition2.Id, value = values[0] });
            }

            initialized = true;
        }
        #endregion

        #region Tests
        [Test(Description = "Verify if EnsureData() executes well")]
        public void Test_EnsureData()
        {
            EnsureData();
            Assert.IsTrue(initialized);
            Assert.That(_userType, !Is.Null);
            Assert.That(_dataTypeDefinition1, !Is.Null);
            Assert.That(_dataTypeDefinition1.Id, !Is.EqualTo(0));
            Assert.That(_dataTypeDefinition2, !Is.Null);
            Assert.That(_dataTypeDefinition2.Id, !Is.EqualTo(0));
            traceCompletion();
        }

        [Test(Description = "Test static CountOfPreValues(int dataTypeDefId) method")]
        public void Test_CountOfPreValues()
        {
            EnsureData();
            Assert.That(PreValues.CountOfPreValues(_dataTypeDefinition1.Id), Is.EqualTo(_dataTypeDefinition1_PrevaluesTestCount));
            Assert.That(PreValues.CountOfPreValues(_dataTypeDefinition2.Id), Is.EqualTo(_dataTypeDefinition2_PrevaluesTestCount));
            traceCompletion();
        }

        [Test(Description = "Test static DeleteByDataTypeDefinition(int dataTypeDefId) method")]
        public void Test_DeleteByDataTypeDefinition()
        {
            EnsureData();
            PreValues.DeleteByDataTypeDefinition(_dataTypeDefinition1.Id);
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition1.Id).Count, Is.EqualTo(0));
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition2.Id).Count, Is.EqualTo(_dataTypeDefinition2_PrevaluesTestCount));

            PreValues.DeleteByDataTypeDefinition(_dataTypeDefinition2.Id);
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition2.Id).Count, Is.EqualTo(0));

            traceCompletion();
        }

        [Test(Description = "Test static GetPreValues(int DataTypeId) method")]
        public void Test_GetPreValues()
        {
            EnsureData();
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition1.Id).Count, Is.EqualTo(_dataTypeDefinition1_PrevaluesTestCount));
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition2.Id).Count, Is.EqualTo(_dataTypeDefinition2_PrevaluesTestCount));
            traceCompletion();
        }

        #endregion
            
    }
}
