Budgeteer
=========

##Contains all code for the WP client

Because of the way Xamarin works, the WP client has to be in its own separate solution. This means that it contains a reference to Budgeteer.dll from the main solution. Any other assemblies from the main solution must also be referenced manually, and the references must be changed depending on whether you want to run it using a DEBUG or REALEASE version of the assembly. This also means that changes to the main solution will NOT show up in the WP client before the main solution has been built in Xamarin Studio.