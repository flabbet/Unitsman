# What is Unitsman?

Unitsman is a lightweighted program that deals very smartly with converting units.

# Requirements

Unitsman requires `.NET Core 3.1` installed.
To build open .sln file in Visual Studio 2019 for example.

# Usage

`Unitsman <value> <unit> <targetUnit>` for example
`Unitsman 50 km m` - this will convert 50 kilometers to meters

For conversion precision use `-d <0-15>` or `--decimals <0-15>`

Note: This is a CLI application so it needs to be run in command prompt or terminal

# How smart is it?

Unitsman analyzes unit files very deeply. If there isn't defined a unit but any other references to it, it gets converted. You can check this out by typing `5 "nautical mile" inch`, There isn't nautical mile in Units, but Meter unit is referencing to nautical mile, so it converts: inch -> meter -> nautical mile

Also you don't have to worry about SI prefixes. If there is meter defined, program automatically will recognize km, cm, mm etc. **make sure to use symbols, not names if using SI prefixes**.

Unitsman also deals with complex conversions. At the moment you can smartly convert for example m/s to any length/time unit, you only have to make sure that there is meters per second defined in units, second type can be anything that matches same formula. 

For now smart conversions only work for simpler complex types like meters per second. Of course if defined, any complex unit can be converted.
To try it out type `30 m/s in/ms`, this will convert 30 meters per second to inches per millisecond, inches can be anything, same as milliseconds.

# Installing new units

Program loads units from Units folder. This folder contains .json files in which unit data is defined. To add new units simply copy json files with new units to Units folder.


# Creating new Units

Units .jsons are pretty simple to understand, check out existing ones for reference.
