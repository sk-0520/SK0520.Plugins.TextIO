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

function ReplaceElement([hashtable] $map, [xml] $xml, [string] $targetXpath, [string] $parentXpath, [string] $elementName) {
    $element = $xml.SelectSingleNode($targetXpath);
    if ($null -ne $element) {
        $val = $element.InnerText
        foreach ($key in $map.keys) {
            $val = $val.Replace($key, $map[$key])
        }
        $element.InnerText = $val
    }
}

$projectXml = [XML](Get-Content $ProjectFilePath  -Encoding UTF8)

InsertElement $Revision $projectXml '/Project/PropertyGroup[1]/InformationalVersion[1]' '/Project/PropertyGroup[1]' 'InformationalVersion'

$repMap = @{
    '@YYYY@' = (Get-Date).Year
    '@NAME@' = 'sk'
    '@SITE@' = 'content-type-text.net'
}
#ReplaceElement $repMap $projectXml '/Project/PropertyGroup[1]/Copyright[1]' '/Project/PropertyGroup[1]' 'Copyright'

$projectCommonXml.Save($projectXml)

$projectXml.Save($ProjectFilePath)

