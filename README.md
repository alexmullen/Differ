# Differ

Find the specific differences in objects.

## Overview

Differ is a library for finding all specific differences within two objects. 

## Why

For cases where you have two versions of the same object representing a snapshot of state at a point in time and you need to view the differences between these two states. 

You could write a chain of if-elseif boilerplate, but this introduces coupling, is brittle and needs updating whenever a new attribute is added.

This is where Differ comes in.

## Usage
### Typical example

```
var firstPerson = new Person
{
    Id = 1,
    Name = "John",
    Age = 25,
    Address = new PersonAddress
    {
        AddressLine = "13 Acme oad",
        City = "Acme",
    }
};

var secondPerson = new Person
{
    Id = 1,
    Name = "John",
    Age = 26,
    Address = new PersonAddress
    {
        AddressLine = "13 Acme Road",
        City = "Acme",
    }
};

var differences = Differentiator.Differentiate(firstPerson, secondPerson);
```

In English, the resulting `differences` array can interpreted as:

* Age changed from '25' to '26'
* Address changed
    * AddressLine changed from '13 Acme oad' to '13 Acme Road'

### Ignoring fields and properties

Often times, some attributes should be ignored as they do not constitute a fundamental difference to the object. For example, the class definition for the `Person` object used in the previous example could be pulled from a database with a `LastUpdated` metadata field which is not typically changed directly by a user and needs to be ignored for comparison. This can be done be adding the `[NonDifferable]` attribute to this member like so.
```
class Person
{
    public int Id { get; set; }
    public int Age { get; set; }
    public string? Name { get; set; }
    public PersonAddress? Address { get; set; }
    [NonDifferable]
    public DateTime LastUpdated { get; set; }
}
```

### Whitelisting fields and properties
Sometimes it makes sense to explicitly whitelist the members. Use `[Differable]` in this case. 
```
class SomeComplexFormModel 
{
    public int Id { get; set; }
    [Differable]
    public string Value { get; set; }
    public bool _valueIsValid { get; }
    public string _metadata { get; }
    public DateTime _lastUpdated { get; }
    ...
}
```

### Objects in collections
To correctly identify changes within a collection of non-primitives, the collection object type needs to have its key explicitly identified. Use `[Key]` on the fields and properties that make up the objects unique key.

```
class User
{
    [Key]
    public int Id;
    ...
}
```

Sometimes an object can only be uniquely identified with more than one attribute. Simply use `[Key]` on each field to identify the composite key.

```
class Document
{
    [Key]
    public int Id;
    [Key]
    public int Version;
    ...
}
```

## Limitations
### Item arrangement
Item arrangement within a collection is currently not considered a difference. The following code will result in an empty result collection.
```
var firstCollection = new int[]  { 1, 2, 3 };
var secondCollection = new int[] { 1, 3, 2 };

var differences = Differentiator.Differentiate(firstCollection, secondCollection);
// differences.Count == 0
```
### Circular references
Circular object references are currently not completely handled and will result in infinite recursion.

## License
Licensed under the Apache License, Version 2.0