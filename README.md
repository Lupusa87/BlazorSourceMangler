# Blazor Source Mangler

This is .core console app which is processing blazor dlls (only blazor app and blazor lib dlls, not common dlls like mscorelib) and is mangling propery/field/parameter/method/type/namespace names.

Purpose is to make downloaded balzor dlls less readable for foreign eyes.

Also shortening names is giving some small dll's size reducion.

App is using mono.cecil.

Any PRs are welcome.
