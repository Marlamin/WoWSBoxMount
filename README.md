# WoW s&box mount
While s&box allows me to do silly things like mounting WoW, I'm going to do exactly that! 

## Libraries
I haven't figured out how libraries work in s&box work yet, so (parts of) these libraries are used (but gitignore'd):
- https://github.com/wowdev/TACTSharp/tree/main/TACTSharp
- https://github.com/wowdev/BLPSharp/tree/master/BLPSharp
- https://github.com/Marlamin/WoWFormatLib/tree/master/WoWFormatLib 
- https://github.com/jandk/TinyBCSharp/tree/main/TinyBCSharp

## Getting it to work
Various changes to get stuff to compile/not having to include the full libraries are required, especially when doing so through the s&box editor as TACTSharp uses unsafe code. I'm just using Visual Studio without the editor and testing it in s&box's Sandbox mode. 

You will also have to change the output path of the DLL in the .csproj.