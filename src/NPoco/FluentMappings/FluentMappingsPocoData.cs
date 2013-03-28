﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NPoco.FluentMappings
{
    public class FluentMappingsPocoData : PocoData
    {
        public FluentMappingsPocoData(Type t, TypeDefinition typeConfig, IMapper mapper)
        {
            Mapper = mapper;
            type = t;
            TableInfo = new TableInfo();

            // Get the table name
            var a = typeConfig.TableName ?? "";
            TableInfo.TableName = a.Length == 0 ? t.Name : a;

            // Get the primary key
            a = typeConfig.PrimaryKey ?? "";
            TableInfo.PrimaryKey = a.Length == 0 ? "ID" : a;

            a = typeConfig.SequenceName ?? "";
            TableInfo.SequenceName = a.Length == 0 ? null : a;

            TableInfo.AutoIncrement = typeConfig.AutoIncrement ?? true;

            // Set autoincrement false if primary key has multiple columns
            TableInfo.AutoIncrement = TableInfo.AutoIncrement ? !TableInfo.PrimaryKey.Contains(',') : TableInfo.AutoIncrement;

            // Call column mapper
            if (mapper != null)
                mapper.GetTableInfo(t, TableInfo);

            // Work out bound properties
            bool explicitColumns = typeConfig.ExplicitColumns ?? false;
            Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
            foreach (var pi in t.GetProperties())
            {
                // Work out if properties is to be included
                var isColumnDefined = typeConfig.ColumnConfiguration.ContainsKey(pi.Name);
                if (explicitColumns)
                {
                    if (!isColumnDefined)
                        continue;
                }
                else
                {
                    if (isColumnDefined && (typeConfig.ColumnConfiguration[pi.Name].IgnoreColumn.HasValue && typeConfig.ColumnConfiguration[pi.Name].IgnoreColumn.Value))
                        continue;
                }

                var pc = new PocoColumn();
                pc.PropertyInfo = pi;

                // Work out the DB column name
                if (isColumnDefined)
                {
                    var colattr = typeConfig.ColumnConfiguration[pi.Name];
                    pc.ColumnName = colattr.DbColumnName;
                    if (colattr.ResultColumn.HasValue && colattr.ResultColumn.Value)
                        pc.ResultColumn = true;
                    else if (colattr.VersionColumn.HasValue && colattr.VersionColumn.Value)
                        pc.VersionColumn = true;

                    if (colattr.ForceUtc.HasValue && colattr.ForceUtc.Value)
                        pc.ForceToUtc = true;

                    if (TableInfo.PrimaryKey.Split(',').Contains(pi.Name))
                        TableInfo.PrimaryKey = (pc.ColumnName ?? pi.Name) + ",";

                    pc.ColumnType = colattr.DbColumnType;

                }
                if (pc.ColumnName == null)
                {
                    pc.ColumnName = pi.Name;
                    if (mapper != null && !mapper.MapPropertyToColumn(pi, ref pc.ColumnName, ref pc.ResultColumn))
                        continue;
                }

                // Store it
                Columns.Add(pc.ColumnName, pc);
            }

            // Trim trailing slash if built using Property names
            TableInfo.PrimaryKey = TableInfo.PrimaryKey.TrimEnd(',');

            // Build column list for automatic select
            QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();

        }
    }
}
