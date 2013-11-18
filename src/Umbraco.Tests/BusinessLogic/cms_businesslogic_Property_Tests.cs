﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
//using Umbraco.Core.Models;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.interfaces;

namespace Umbraco.Tests.BusinessLogic
{

    [TestFixture]
    public class cms_businesslogic_Property_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Property_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_Property_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_contentType1, !Is.Null);
            Assert.That(_contentType2, !Is.Null);
            Assert.That(_propertyTypeGroup1, !Is.Null);
            Assert.That(_propertyTypeGroup2, !Is.Null);
            Assert.That(_propertyTypeGroup3, !Is.Null);
            Assert.That(_propertyType1, !Is.Null);
            Assert.That(_propertyType2, !Is.Null);
            Assert.That(_propertyType3, !Is.Null);
            Assert.That(_propertyData1, !Is.Null);
            Assert.That(_propertyData2, !Is.Null);
            Assert.That(_propertyData3, !Is.Null);
            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAll_Property_TestRecordsAreDeleted();

            Assert.That(getDto<PropertyDataDto>(_propertyData1.Id), Is.Null);
            Assert.That(getDto<PropertyDataDto>(_propertyData2.Id), Is.Null);
            Assert.That(getDto<PropertyDataDto>(_propertyData3.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType1.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType2.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType3.Id), Is.Null);
            Assert.That(getDto<ContentTypeDto>(_contentType1.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(getDto<ContentTypeDto>(_contentType2.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup1.Id), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup2.Id), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup3.Id), Is.Null);

            Assert.That(getDto<NodeDto>(_node1.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node2.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node3.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node4.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors")]
        public void _2nd_Test_Property_Constructors()
        {
            // !!!!!!!!!!!!!!!!!
            // ! constuctor fails
            // !!!!!!!!!!!!!!!!!
            //  Property class constructor is failing because of unclear reasons. 
            //  Could be that last part of its code is obsolete. 
            //  Suppressed by try {} catch {} block in constructor code.
            //  Should be carefully investigated and solved later.
            var property = new Property(_propertyData1.Id);
            var savedPropertyDto = getDto<PropertyDataDto>(_propertyData1.Id);

            assertPropertySetup(property, savedPropertyDto);  
        }

        private void assertPropertySetup(Property testProperty, PropertyDataDto savedPropertyDto)
        {
            Assert.That(testProperty.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");
            Assert.That(testProperty.VersionId, Is.EqualTo(savedPropertyDto.VersionId), "Version test failed");
            Assert.That(testProperty.PropertyType.Id, Is.EqualTo(savedPropertyDto.PropertyTypeId), "PropertyTypeId test failed");
        }

        [Test(Description = "Test 'public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)' method")]
        public void Test_Property_MakeNew()
        {
            // public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)

            var propertyType = new PropertyType(_propertyType1.Id);
            Assert.That(propertyType, !Is.Null);   

            var content = new Content(_node1.Id);
            Assert.That(content, !Is.Null);   

            // ! Property constructor called in MakeNew fails
            var property = Property.MakeNew(propertyType, content, Guid.NewGuid());
            var savedPropertyDto = getDto<PropertyDataDto>(property.Id);

            assertPropertySetup(property, savedPropertyDto);  
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_Property_Delete()
        {
            var property = new Property(_propertyData1.Id);
            Assert.That(property, !Is.Null);

            var savedPropertyDto = getDto<PropertyDataDto>(property.Id);
            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");

            property.delete();

            var savedPropertyDto2 = getDto<PropertyDataDto>(property.Id);
            Assert.That(savedPropertyDto2, Is.Null);

            initialized = false;
        }

    }
}
