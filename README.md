# Project FNZ
The new main project with transformation of our architecture. Client, server, shared and level editor are now in different assemblies. This means we can easily build a headless server and a much clearer separatio between client and server will result in less strange bugs. We can also use DOTS on the server since it will now be run on the unity main thread.
