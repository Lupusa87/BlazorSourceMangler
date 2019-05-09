# Blazor Source Mangler

App is not tested for latest commit and is in active development.

This is .net core console app which is processing blazor dlls (only blazor app and blazor lib dlls, not common dlls like mscorlib) and is mangling property/field/parameter/method/type/namespace names and cleaning dead codes.

Purpose is to make downloaded blazor dlls smaller and less readable for foreign eyes.

Also shortening names is giving some additional dll's size reducion.

App is using mono.cecil.


Check this [youtube video](https://www.youtube.com/watch?v=nlXax81b1UE) for more details.

Check this [blazor todos app](https://lupblazortodo.z20.web.core.windows.net) to see result of this app (downloaded blazortodos.dll is mangled and decompilation shows uglyfied code).

Final goal is to use this tool for blazor common dlls too, to get more significant size reduction.

Any PRs are welcome.
