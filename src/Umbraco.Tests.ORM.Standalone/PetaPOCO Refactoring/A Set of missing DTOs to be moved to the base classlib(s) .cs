using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace umbraco.cms.businesslogic.datatype
{
        #region Private methods
        [TableName("cmsDataTypePreValues")]
        [PrimaryKey("id")]
        [ExplicitColumns]
        internal class PreValueDto
        {
            [Column("Id")]
            [PrimaryKeyColumn(IdentitySeed = 1)]
            public int Id { get; set; }
            [Column("SortOrder")]
            public int SortOrder { get; set; }
            [Column("Value")]
            public string Value { get; set; }
            [Column("dataTypeNodeId")]
            public int DataTypeId { get; set; }  // source DataTypeNodeId
        }
        #endregion
}

namespace umbraco.cms.businesslogic.packager
{

    #region PackageDto // move to Umbraco.Core.Models.Rdbms
    [TableName("umbracoInstalledPackages")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class InstalledPackageDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("uninstalled")]
        [Constraint(Default = "0")]
        public bool Uninstalled { get; set; }

        [Column("upgradeId")]
        public int UpgradeId { get; set; }
        [Column("installDate")]
        public DateTime InstallDate { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("package")]
        public Guid PackageId { get; set; }
        [Column("versionMajor")]
        public int VersionMajor { get; set; }
        [Column("versionMinor")]
        public int VersionMinor { get; set; }
        [Column("versionPatch")]
        public int VersionPatch { get; set; }
    }
    #endregion
}
