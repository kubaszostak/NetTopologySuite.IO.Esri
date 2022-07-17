﻿using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Shp.Readers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetTopologySuite.IO.Esri.Test.Various
{
    /// <summary>
    /// Tests for samples included int the README.md
    /// </summary>
    internal class ReadmeSamples
    {
        private readonly string dbfPath;
        private readonly string shpPath;

        public ReadmeSamples()
        {
            shpPath = TestShapefiles.PathTo("fields_utf8.shp");
            dbfPath = Path.ChangeExtension(shpPath, ".dbf");
        }

        [Test]
        public void DbfReader()
        {
            using (var dbf = new DbfReader(dbfPath))
            {
                foreach (var record in dbf)
                {
                    var fieldNames = record.Keys;
                    foreach (var fieldName in fieldNames)
                    {
                        Console.WriteLine($"{fieldName,10} {record[fieldName]}");
                    }
                    Console.WriteLine();
                }
            }
        }

        [Test]
        public void ShpReader()
        {
            using (var shpStream = File.OpenRead(shpPath))
            using (var shp = new ShpPointReader(shpStream))
            {
                while (shp.Read())
                {
                    Console.WriteLine(shp.Geometry);
                }
            }
        }

        [Test]
        public void ShapefileReader()
        {
            foreach (var feature in Shapefile.ReadAllFeatures(shpPath))
            {
                foreach (var attrName in feature.Attributes.GetNames())
                {
                    Console.WriteLine($"{attrName,10}: {feature.Attributes[attrName]}");
                }
                Console.WriteLine($"     SHAPE: {feature.Geometry}");
                Console.WriteLine();
            }
        }

        [Test]
        public void ShapefileWriter()
        {
            var shpPath = TestShapefiles.GetTempShpPath();

            var features = new List<Feature>();
            for (int i = 1; i < 5; i++)
            {
                var lineCoords = new List<CoordinateZ>();
                lineCoords.Add(new CoordinateZ(i, i + 1, i));
                lineCoords.Add(new CoordinateZ(i, i, i));
                lineCoords.Add(new CoordinateZ(i + 1, i, i));
                var line = new LineString(lineCoords.ToArray());
                var mline = new MultiLineString(new LineString[] { line });

                var attributes = new AttributesTable();
                attributes.Add("date", new DateTime(2000, 1, i + 1));
                attributes.Add("float", i * 0.1);
                attributes.Add("int", i);
                attributes.Add("logical", i % 2 == 0);
                attributes.Add("text", i.ToString("0.00"));

                var feature = new Feature(mline, attributes);
                features.Add(feature);
            }

            Shapefile.WriteAllFeatures(features, shpPath);

            TestShapefiles.DeleteShp(shpPath);
        }

    }
}
