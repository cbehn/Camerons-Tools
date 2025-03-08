using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MyNamespace
{
    public class CabinetBoxSkeleton : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CabinetBoxSkeleton class.
        /// </summary>
        public CabinetBoxSkeleton()
          : base("CabinetBoxSkeleton", "Skeleton",
              "A cabinet skeleton made with all solid sides",
              "Cabinetry", "Base")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "W", "Width of the cabinet", GH_ParamAccess.item, 36);
            pManager.AddNumberParameter("Depth", "D", "Depth of the cabinet", GH_ParamAccess.item, 24);
            pManager.AddNumberParameter("Height", "H", "Height of the cabinet", GH_ParamAccess.item, 36);
            pManager.AddNumberParameter("Thickness", "T", "Thickness of the cabinet sides", GH_ParamAccess.item, 0.75);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cabinet", "C", "Cabinet skeleton", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            float width = 0.0;
            var depth = 0.0;
            var height = 0.0;
            var thickness = 0.0;
            
            if (!DA.GetData(0, ref width) || !DA.GetData(1, ref depth) || !DA.GetData(2, ref height) || !DA.GetData(3, ref thickness)) return;



        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3F8F548A-617D-4359-8D2B-E5564E77E2E5"); }
        }
    }
}