Param(
	[Parameter(mandatory = $true)][string] $Revision,
	[Parameter(mandatory = $true)][string] $ProjectFilePath
)

function InsertElement([string] $value, [xml] $xml, [string] $targetXpath, [string] $parentXpath, [string] $elementName) {
    $element = $xml.SelectSingleNode($targetXpath);
    if ($null -eq $element) {
        $propGroup = $xml.SelectSingleNode($parentXpath)
        $element = $xml.CreateElement($elementName);
        $propGroup.AppendChild($element) | Out-Null;
        $element.InnerText = $value
    }
}

echo "Revision: $Revision"
echo "ProjectFilePath: $ProjectFilePath"

$projectXml = [XML](Get-Content $ProjectFilePath  -Encoding UTF8)

InsertElement $Revision $projectXml '/Project/PropertyGroup[1]/InformationalVersion[1]' '/Project/PropertyGroup[1]' 'InformationalVersion'

$projectXml.Save($ProjectFilePath)

