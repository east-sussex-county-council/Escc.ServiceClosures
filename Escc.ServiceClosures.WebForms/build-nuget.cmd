@echo off
set nuspec="%1"
set nuspec=%nuspec:\=\\%
nuget pack "%nuspec%Escc.ServiceClosures.WebForms.nuspec"
nuget pack "%nuspec%Escc.ServiceClosures.Rss.nuspec"