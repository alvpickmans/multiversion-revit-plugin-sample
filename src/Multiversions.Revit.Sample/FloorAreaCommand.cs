using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Multiversions.Revit.Sample
{
    /// <summary>
    /// A sample command.
    /// </summary>
    /// <seealso cref="T:Autodesk.Revit.UI.IExternalCommand" />
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FloorAreaCommand : IExternalCommand
    {
        /// <summary>
        /// Executes the specified Revit command <see cref="ExternalCommand"/>.
        /// The main Execute method (inherited from IExternalCommand) must be public.
        /// </summary>
        /// <param name="commandData">The command data / context.</param>
        /// <param name="message">The message.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>The result of command execution.</returns>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var selection = uidoc.Selection;

            // Prompts the user to select a single element of type Floof
            var floorRef = selection.PickObject(ObjectType.Element, new FloorSelectionFilter());
            var floor = doc.GetElement(floorRef) as Floor;

            // Gets the computed floor area in square feet
            var areaInternal = floor
                .get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED)
                .AsDouble();

            double? areaMetric = null;

            // Using compilation conditional statements, we can
            // define which code will get compiled depending on the target revit version
            // We decided to create out custom 'USE_FORGETYPEID' so we can decide which Revit versions
            // to use in a single place (csproj)
            // If we decide not to use a custom preprocessor tag, the below if statemente would have been:
            // #if !(REVIT2018 | REVIT2019 | REVIT 2020)
#if USE_FORGETYPEID
            var forgeTypeId = new ForgeTypeId(UnitTypeId.SquareMeters.TypeId);
            areaMetric = UnitUtils.ConvertFromInternalUnits(areaInternal, forgeTypeId);
#else

            areaMetric = UnitUtils.ConvertFromInternalUnits(areaInternal, DisplayUnitType.DUT_SQUARE_METERS);
#endif

            TaskDialog.Show("Multiversion Sample", $"Selected floor has a surface area of {areaMetric} m2");
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Selection filter class that only allows to select <see cref="Floor"/> elements.
    /// </summary>
    internal class FloorSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
            => typeof(Floor).IsAssignableFrom(elem.GetType());

        public bool AllowReference(Reference reference, XYZ position)
            => true;
    }
}