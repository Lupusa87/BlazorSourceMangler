# Blazor Source Mangler

This is .core console app which is processing blazor dlls (only blazor app and blazor lib dlls, not common dlls like mscorelib) and is mangling property/field/parameter/method/type/namespace names.

Purpose is to make downloaded balzor dlls less readable for foreign eyes.

Also shortening names is giving some small dll's size reducion.

App is using mono.cecil.


Check this [youtube video](https://www.youtube.com/watch?v=nlXax81b1UE) for more details.

Check this [blazor todos app](https://lupblazortodo.z20.web.core.windows.net) to see result of this app (downloaded blazortodos.dll is mangled and decompilation shows uglyfied code).

Any PRs are welcome.
