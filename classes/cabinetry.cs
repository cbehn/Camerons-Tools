using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Camerons_Tools.Cabinetry
{
    public class CabinetSkeleton<T> : GH_GeometricGoo<T>
    {
        // Properties for cabinet dimensions
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double Thickness { get; set; }

        public Plane Plane { get; set; }

        // Lists to store parts and their names
        public List<Brep> Parts { get; set; } = new List<Brep>();
        public List<string> PartNames { get; set; } = new List<string>();

        // Private backing fields for Profiles
        private List<Curve> _profiles = new List<Curve>();
        private Dictionary<string, Curve> _namedProfiles = new Dictionary<string, Curve>();

        // Public property for accessing profiles
        public List<Curve> Profiles
        {
            get { return _profiles; }
            set
            {
                _profiles = value;
                // Update the named profiles dictionary when the main list changes
                UpdateNamedProfiles();
            }
        }

        // Indexer for accessing profiles by name
        public Curve this[string name]
        {
            get
            {
                if (_namedProfiles.ContainsKey(name))
                {
                    return _namedProfiles[name];
                }
                else
                {
                    return null; // Or throw an exception if you prefer
                }
            }
        }

        public CabinetSkeleton()
        {
            // Initialize default values
            Width = 0;
            Height = 0;
            Depth = 0;
            Thickness = 0;

            Plane = Plane.WorldXY;
        }

        // Constructor with parameters
        public CabinetSkeleton(double width, double height, double depth, double thickness, Plane plane)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Thickness = thickness;
            Plane = plane;
        }

        //Add a component to the skeleton
        public void AddPart(Brep part, string name)
        {
            Parts.Add(part);
            PartNames.Add(name);
        }

        public override BoundingBox Boundingbox
        {
            get
            {
                // Calculate bounding box based on the parts in the 'Parts' list.
                if (Parts.Count == 0)
                {
                    return BoundingBox.Unset; // Return an unset bounding box if there are no parts.
                }

                BoundingBox combinedBoundingBox = Parts[0].GetBoundingBox(false); // Start with the bounding box of the first part.

                for (int i = 1; i < Parts.Count; i++)
                {
                    combinedBoundingBox.Union(Parts[i].GetBoundingBox(false)); // Union with each subsequent part.
                }

                return combinedBoundingBox;
            }
        }

        public override string TypeName => "CabinetSkeleton";

        public override string TypeDescription => "Represents a skeleton of a cabinet";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            //implement this so it will duplicate correctly.
            // Create a new CabinetSkeleton object.
            CabinetSkeleton<T> duplicate = new CabinetSkeleton<T>(Width, Height, Depth, Thickness, Plane);

            // Copy the parts and their names to the duplicate object.
            for (int i = 0; i < Parts.Count; i++)
            {
                duplicate.AddPart(Parts[i].DuplicateBrep(), PartNames[i]);
            }

            foreach (var curve in Profiles)
            {
                duplicate.Profiles.Add(curve.DuplicateCurve());
            }

            return duplicate;
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Parts.Count == 0)
            {
                return BoundingBox.Unset; // Return an unset bounding box if there are no parts.
            }
            List<BoundingBox> bBoxes = new List<BoundingBox>();
            foreach (Brep item in Parts)
            {
                bBoxes.Add(item.GetBoundingBox(xform));
            }
            BoundingBox combinedBoundingBox = bBoxes[0];
            for (int i = 1; i < bBoxes.Count; i++)
            {
                combinedBoundingBox.Union(bBoxes[i]); // Union with each subsequent part.
            }

            return combinedBoundingBox;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            // Create a new CabinetSkeleton object.
            CabinetSkeleton<T> morphed = new CabinetSkeleton<T>(Width, Height, Depth, Thickness, Plane);
            for (int i = 0; i < Parts.Count; i++)
            {
                morphed.AddPart(Parts[i].DuplicateBrep(), PartNames[i]);
            }

            return morphed;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            CabinetSkeleton<T> transformed = new CabinetSkeleton<T>(Width, Height, Depth, Thickness, Plane);
            for (int i = 0; i < Parts.Count; i++)
            {
                transformed.AddPart(Parts[i].DuplicateBrep().Transform(xform), PartNames[i]);
            }
            foreach (var curve in Profiles)
            {
                transformed.Profiles.Add(curve.DuplicateCurve().Transform(xform));
            }
            return transformed;
        }

        public override string ToString()
        {
            return "CabinetSkeleton object";
        }

        public override bool IsValid => true;

        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(List<Curve>)))
            {
                target = (Q)(object)Profiles;
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(List<Brep>)))
            {
                target = (Q)(object)Parts;
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(Brep)))
            {
                // Combine all breps into a single brep.
                Brep combinedBrep = new Brep();
                foreach (var brep in Parts)
                {
                    combinedBrep.Append(brep);
                }
                target = (Q)(object)combinedBrep;
                return true;
            }
            return base.CastTo(ref target);
        }

        public override bool CastFrom(object source)
        {
            if (source is Brep brep)
            {
                // Create a new part and name and assign it
                AddPart(brep, "New Brep");
                return true;
            }

            if (source is List<Brep> breps)
            {
                foreach (var brepItem in breps)
                {
                    AddPart(brepItem, "New Brep");
                }
                return true;
            }

            if (source is Curve curve)
            {
                Profiles.Add(curve);
                return true;
            }

            if (source is List<Curve> curves)
            {
                Profiles = curves;
                return true;
            }
            return base.CastFrom(source);
        }

        // Helper method to update the named profiles dictionary
        private void UpdateNamedProfiles()
        {
            _namedProfiles.Clear();
            // Example: Assuming the first curve is "left," the second is "right," etc.
            if (_profiles.Count > 0) _namedProfiles["left"] = _profiles[0];
            if (_profiles.Count > 1) _namedProfiles["right"] = _profiles[1];
            if (_profiles.Count > 2) _namedProfiles["top"] = _profiles[2];
            if (_profiles.Count > 3) _namedProfiles["bottom"] = _profiles[3];
            if (_profiles.Count > 4) _namedProfiles["front"] = _profiles[4];
            if (_profiles.Count > 5) _namedProfiles["back"] = _profiles[5];
            // ... add more as needed
        }
    }
}
