﻿using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Shp.Readers;
using System.IO;
using System.Linq;

namespace NetTopologySuite.IO.Esri.Shp
{
    /// <summary>
    /// Base class class for reading and writing a fixed-length file header and variable-length records from a *.SHP file.
    /// </summary>
    public abstract class Shp : ManagedDisposable
    {
        internal readonly bool HasM;
        internal readonly bool HasZ;

        /// <summary>
        /// Shape type.
        /// </summary>
        public ShapeType ShapeType { get; }


        /// <summary>
        /// Initializes a new instance of the reader class.
        /// </summary>
        /// <param name="shapeType">Shape type.</param>
        public Shp(ShapeType shapeType)
        {
            ShapeType = shapeType;
            if (ShapeType == ShapeType.NullShape)
            {
                ThrowUnsupportedShapeTypeException();
            }

            HasM = shapeType.HasM();
            HasZ = shapeType.HasZ();
        }

        internal void ThrowUnsupportedShapeTypeException()
        {
            throw new FileLoadException(GetType().Name + $" does not support {ShapeType} shapes.");
        }

        #region Static Methods

        /// <summary>
        /// Opens SHP reader.
        /// </summary>
        /// <param name="shpStream">SHP file stream.</param>
        /// <param name="factory">Geometry factory.</param>
        /// <returns>SHP reader.</returns>
        public static ShpReader OpenRead(Stream shpStream, GeometryFactory factory = null)
        {
            var shapeType = Shapefile.GetShapeType(shpStream);

            if (shapeType.IsPoint())
            {
                return new ShpPointReader(shpStream, factory);
            }
            else if (shapeType.IsMultiPoint())
            {
                return new ShpMultiPointReader(shpStream, factory);
            }
            else if (shapeType.IsPolyLine())
            {
                return new ShpPolyLineReader(shpStream, factory);
            }
            else if (shapeType.IsPolygon())
            {
                return new ShpPolygonReader(shpStream, factory);
            }
            else
            {
                throw new FileLoadException("Unsupported shapefile type: " + shapeType);
            }
        }


        /// <summary>
        /// Reads all geometries from SHP file.
        /// </summary>
        /// <param name="shpPath">Path to SHP file.</param>
        /// <param name="factory">Geometry factory.</param>
        /// <returns>Shapefile geometries.</returns>
        public static Geometry[] ReadAllGeometries(string shpPath, GeometryFactory factory = null)
        {
            shpPath = Path.ChangeExtension(shpPath, ".shp");
            using (var shpStream = File.OpenRead(shpPath))
            {
                var shp = OpenRead(shpStream, factory);
                return shp.ToArray();
            }
        }

        #endregion
    }
}
