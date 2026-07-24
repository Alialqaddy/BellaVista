using Umbraco.Cms.Core.PropertyEditors;

namespace BellaVista.Composing;

/// <summary>
/// Thin server-side registration for the custom Spice Level property editor. The
/// actual editing experience (chili icons, AngularJS controller) lives entirely in
/// App_Plugins/SpiceLevel per the course's package.manifest pattern; this class only
/// exists so the editor has a real <see cref="IDataEditor"/> in the DataEditorCollection,
/// which Umbraco needs to create a Data Type for it (whether that Data Type is created
/// by hand in the backoffice, or - as here - seeded in code on startup).
/// </summary>
[DataEditor(
    "BellaVista.SpiceLevel",
    EditorType.PropertyValue,
    "Spice Level",
    "~/App_Plugins/SpiceLevel/spicelevel.editor.html",
    ValueType = ValueTypes.Integer,
    Icon = "icon-fire")]
public class SpiceLevelDataEditor : DataEditor
{
    public SpiceLevelDataEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
    }
}
