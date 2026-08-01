namespace EmberConfig.PrefabDataGen.Extraction;

using EmberConfig.PrefabDataGen.Parsing;
using YamlDotNet.RepresentationModel;

internal static class ComponentPredicates
{
    internal static bool IsImage(ComponentNode component) =>
        component.TypeName == "UnityEngine.UI.Image" ||
        (component.TypeName == "MonoBehaviour" &&
         component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
         component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type")));

    internal static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    internal static bool IsToggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_IsOn")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("toggleTransition"));

    internal static bool IsM1Toggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_IsOn")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Group"));

    internal static bool IsDyCtrlDropDownScrollRect(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("dropdown")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("srviewport")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("srcontent"));

    internal static bool IsHorizontalLayoutGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Spacing")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_ChildAlignment"));

    internal static bool IsToggleGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_AllowSwitchOff"));

    internal static bool IsControllerLinkToggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("LinkedDropDown")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("buttontext"));

    internal static bool IsM1Button(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_TargetGraphic")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_OnClick"));

    internal static bool IsSlider(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_FillRect")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_HandleRect"));

    internal static bool IsAkTriggerMouseUp(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("triggerList")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("eventIdInternal"));
}
