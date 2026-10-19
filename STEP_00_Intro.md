# Introduction

### What is an interpreter ?

Interpreters and compilers are things that are used to instruct a computer how to act according to its user's needs. Differentiating an interpreter from a compiler is less straightforward today because modern implementations of interpreters like JavaScript and Python have a compiler component within them and modern compilers like Java and C# have an interpreter component in their pipeline. 

A simple interpreter is a program that takes a string (source code) as input and performs the actions in that source code one by one. The action can be something like executing a function that prints out the details of weather in Kochi.

A very large portion of developer population have no idea what is going on inside an interpreter. They think interpreters as black boxes that have big nerdy stuff going inside only Einsteins and Curies can understand. This pragmatic guide lets you see inside those black boxes, and understand the intricacies within so that you too can build one when the necessity comes, and it comes way more often than you might think.

The interpreter we are going to build in this book is called ARLang. ARLang will have 4 stages in its interpretation process. You can picturize them forming a pipeline, where one stage's output is the next one's input. 
- Lexical analysis of input string
- Parsing / Generation of Abstract Syntax Tree (AST)
- Semantic Validation of Abstract Syntax Tree (AST)
- AST Traversal and Execution 

These stages can be split into **front end** and **back end**.

Lexical analysis, parsing, and semantic validation together constitute the **front end** of ARLang. Its job begins at reading the source code and ends at producing the AST. This part is mostly platform-independent and generally reusable across platforms.

The **back end** in ARLang is the runtime execution part: it takes the validated AST and performs the program’s behavior which is direct AST evaluation.

So the partitioning criterion for interpreters is:

- **Front end:** “Understand and validate the program.”
- **Back end:** “Execute the program at runtime.”
 
In more advanced VMs like CPython, the back end may include bytecode generation, bytecode interpretation, and/or JIT compilation, but in a simple interpreter like ARLang, the back end is just AST execution.
 
## What is OneOf (Union Type) ?

OneOf is a C# library that lets a variable store one of several possible types, but only one at a time. It helps you write safer code by making sure you handle every possible type.

Think of a lunch box with one compartment and a label: "Sandwich or Fruit". You can put in a sandwich, or you can put in a fruit, but only one of them at a time, and nothing else (no shoes!). Anyone who receives the box knows it holds one of those two things. They can open it and check which one is inside.


```text
LunchBox = Sandwich OR Fruit
```

OneOf works like that lunch box, but for C# types. You decide which types are allowed, and OneOf gives you a type that holds exactly one of them.

Imagine that our program works with two kinds of vehicle: a car and a cycle. A `Vehicle` value can hold either a `Car` or a `Cycle`, but not both at the same time.

```text
Vehicle = Car OR Cycle
```

This is called a union. A few words you will see again and again:

| Word | Meaning in our example |
| --- | --- |
| Union | The box: `Vehicle` |
| Case | One thing the box may hold: `Car` or `Cycle` |
| Value | The one case currently inside the box |

Three rules to remember:

1. The box holds exactly one case at a time.
2. The box can only hold the cases we listed.
3. It is never "half a car and half a cycle".

## Why Use OneOf?

A car and a cycle have some information in common, but also have different properties:

| Type | Properties |
| --- | --- |
| `Car` | Wheel count and cylinder volume in cubic centimetres (cc) |
| `Cycle` | Main wheel count and training-wheel count |

We want one type, `Vehicle`, that can hold either kind without mixing up their properties. A cycle does not need a cylinder-volume property. A car does not need a training-wheel property.

OneOf keeps these types separate and lets us handle both through a single union. We can use `Match()` or `Switch()` to work with the type inside it. In the examples below, we supply a handler for each case so neither kind is forgotten.

### What goes wrong without OneOf?

A common first attempt is to put every property into one class:

```csharp
public class Vehicle
{
    public int WheelCount { get; set; }
    public double? CylinderVolumeCc { get; set; }
    public int? TrainingWheelCount { get; set; }
}
```

The `?` means the value may be empty (`null`). This looks fine at first, but it causes problems:

- A cycle has an empty `CylinderVolumeCc`. A car has an empty `TrainingWheelCount`. Empty values are waiting to cause mistakes.
- Nothing stops someone from creating a vehicle with both a cylinder volume and training wheels. That is not a real car or a real cycle.
- Every time we read a property, we must remember to check whether it is empty. If we forget, the program can crash.
- Looking at the class, we cannot tell which properties belong together.

### How OneOf fixes this

With OneOf, each kind keeps only its own properties, and the `Vehicle` box holds one kind at a time:

```text
Car   -> WheelCount, CylinderVolumeCc
Cycle -> WheelCount, TrainingWheelCount
```

No empty properties. No impossible combinations. When we open the box, C# tells us exactly which kind we have, so we can only read properties that exist.

Inheritance (a base `Vehicle` class with `Car` and `Cycle` classes derived from it) is another valid approach. OneOf is useful when we want to list the allowed cases explicitly in one place: a car or a cycle, and nothing else.

## Create the Two Types

First, define the information that each vehicle stores:

```csharp
public record Car(int WheelCount, double CylinderVolumeCc);
public record Cycle(int WheelCount, int TrainingWheelCount);
```

A record is a simple way to define a type with properties. For example:

```csharp
Car car = new Car(4, 1200);
Cycle cycle = new Cycle(2, 2);
```

The car has four wheels and a cylinder volume of 1200 cc. The cycle has two main wheels and two additional training wheels. Here, `WheelCount` does not include the cycle's training wheels.

## Create a Custom Union

OneOf can be used directly:

```csharp
OneOf<Car, Cycle> vehicle = new Car(4, 1200);
```

But `Vehicle` is a clearer name than repeatedly writing `OneOf<Car, Cycle>`. We can create a named, custom union:

```csharp
using OneOf;

public record Car(int WheelCount, double CylinderVolumeCc);
public record Cycle(int WheelCount, int TrainingWheelCount);

[GenerateOneOf]
public partial class Vehicle : OneOfBase<Car, Cycle>
{
}
```

`OneOfBase<Car, Cycle>` lists the two allowed types. `Vehicle` is the name we give to this union.

Let us read the declaration piece by piece:

| Piece | What it means |
| --- | --- |
| `public partial class Vehicle` | We are creating a class named `Vehicle`. It is `partial`, which means part of it is written somewhere else (by the generator). |
| `: OneOfBase<Car, Cycle>` | `Vehicle` is built on OneOf's base class. The types in `<...>` are the allowed cases. |
| `[GenerateOneOf]` | A request: "Please write the extra code this union needs." |
| `{ }` | The body is empty because we have nothing extra to add. |

The order of the types matters later. Here `Car` is first and `Cycle` is second, so `Match()` and `Switch()` expect the car handler first and the cycle handler second.

Now we can write:

```csharp
Vehicle firstVehicle = new Car(4, 1200);
Vehicle secondVehicle = new Cycle(2, 2);
```

The first value holds a car. The second holds a cycle. Neither `Car` nor `Cycle` needs to inherit from `Vehicle`.

## What Is Source Generation?

A source generator writes extra C# code for us during the build. We write the small union declaration above, and the generator fills in the repetitive parts.

For this example, add these packages to a C# project:

```shell
dotnet add package OneOf
dotnet add package OneOf.SourceGenerator
```

`[GenerateOneOf]` tells the generator which class to extend. `partial` allows our declaration and the generated declaration to make up one class.

The generator adds a constructor and conversion operators. The `Match()` and `Switch()` methods are already provided by `OneOfBase<Car, Cycle>`; they are inherited, not generated for our class.

We do not need to write or maintain that generated code ourselves.

Here is the idea in two steps:

```text
What we write:
    [GenerateOneOf]
    public partial class Vehicle : OneOfBase<Car, Cycle> { }

What the build adds for us:
    - a way to create a Vehicle from a Car
    - a way to create a Vehicle from a Cycle
    - conversions between Car/Cycle and Vehicle
```

It is like ordering a meal kit: we choose the recipe (the two types), and the kitchen (the generator) prepares the repetitive parts.

Without the generator we would have to write all that code by hand for every union. With it, one short declaration is enough.

## Implicit Conversion: From Car or Cycle to Vehicle

Look at this assignment:

```csharp
Vehicle vehicle = new Car(4, 1200);
```

The value on the right is a `Car`. The variable on the left is a `Vehicle`. Why is the assignment allowed?

The source generator adds an implicit conversion operator. In simple terms, it tells C#: "When a `Vehicle` is needed, you may wrap a `Car` in a `Vehicle` automatically."

The generated operator looks like this inside the partial `Vehicle` class:

```csharp
public static implicit operator Vehicle(Car value)
{
    return new Vehicle(value);
}
```

There is a similar operator for `Cycle`. These use C#'s user-defined conversion operators. The car's properties do not change, and the car does not become a cycle. The union simply holds the original car.

Going back to the lunch box: "implicit" means we can just drop the sandwich into the box. We do not need a special tool or a special instruction. The box accepts it because "Sandwich" is on its label.

```text
Car   --(automatic)-->  Vehicle
Cycle --(automatic)-->  Vehicle
```

The conversion is safe because every car is a valid `Vehicle` and every cycle is a valid `Vehicle`. Nothing can be lost or go wrong, which is why C# allows it without a cast.

What if we try to put in something that is not on the label?

```csharp
Vehicle vehicle = "a boat";   // Compile error: string is not Car or Cycle
```

The compiler rejects this before the program runs. That is a big benefit: mistakes are caught early.

This works when passing arguments too:

```csharp
static void ShowWheelCount(Vehicle vehicle)
{
    int wheels = vehicle.Match(
        car => car.WheelCount,
        cycle => cycle.WheelCount
    );

    Console.WriteLine(wheels);
}
```

We can call it with either case directly:

```csharp
ShowWheelCount(new Car(4, 1200));
ShowWheelCount(new Cycle(2, 2));
```

The calls print `4` and `2`. C# automatically applies the appropriate conversion into `Vehicle`.

## Match(): Return a Value

Use `Match()` when you need a result:

```csharp
Vehicle vehicle = new Cycle(2, 2);

int wheels = vehicle.Match(
    car => car.WheelCount,
    cycle => cycle.WheelCount
);

Console.WriteLine(wheels);
```

This prints `2`.

The first lambda, `car => car.WheelCount`, handles a car. The second handles a cycle. Their order follows the order in `OneOfBase<Car, Cycle>`.

Only the matching lambda runs. Because this vehicle holds a cycle, the cycle lambda runs and returns its main wheel count. Both lambdas return an `int`, so `Match()` returns an `int` that we can store in `wheels`.

A lambda is a small inline function. In `car => car.WheelCount`, the part before `=>` is the input (a `Car` named `car`) and the part after is the result.

Here is what happens step by step:

```text
1. vehicle holds a Cycle(2, 2)
2. Match() opens the box and sees: it is a Cycle
3. It skips the car lambda
4. It runs the cycle lambda: cycle.WheelCount -> 2
5. Match() returns 2
```

If the vehicle held a `Car(4, 1200)` instead, step 3 would skip the cycle lambda, and the car lambda would return `4`.

Two things make `Match()` safe:

- We must supply a handler for every case. If we forget the cycle handler, the code will not compile.
- Inside each handler we get the real type, so a car handler can read `CylinderVolumeCc` and a cycle handler can read `TrainingWheelCount`. We cannot mix them up.

Think of `Match()` as asking: "Open the box. If it is a car, tell me this. If it is a cycle, tell me that." We get one answer back.

## Switch(): Perform an Action

Use `Switch()` when you want to do something without returning a result:

```csharp
Vehicle vehicle = new Car(4, 1200);

vehicle.Switch(
    car => Console.WriteLine(
        $"Car: {car.WheelCount} wheels, {car.CylinderVolumeCc} cc"),
    cycle => Console.WriteLine(
        $"Cycle: {cycle.WheelCount} main wheels, {cycle.TrainingWheelCount} training wheels")
);
```

This prints:

```text
Car: 4 wheels, 1200 cc
```

The car lambda receives a `Car`, so it can read `CylinderVolumeCc`. The cycle lambda receives a `Cycle`, so it can read `TrainingWheelCount`.

Again, only one lambda runs. `Switch()` performs that action but returns no value for us to store.

Step by step, for the car above:

```text
1. vehicle holds a Car(4, 1200)
2. Switch() opens the box and sees: it is a Car
3. It runs the car lambda, which prints the line
4. It skips the cycle lambda
5. Nothing is returned
```

Think of `Switch()` as saying: "Open the box. If it is a car, do this. If it is a cycle, do that." We do not ask for an answer. We only ask for something to be done, such as printing, saving, or logging.

Like `Match()`, `Switch()` needs a handler for every case.

## Match() or Switch()?

| Need | Use | Example |
| --- | --- | --- |
| A returned value | `Match()` | Get the wheel count |
| An action with no returned value | `Switch()` | Print the vehicle's details |

Both select the handler for the type currently inside the union. The difference is whether a result comes back.

## Explicit Conversion: From Vehicle Back to Car or Cycle

Going into the union is automatic, but getting a particular type back requires care. A `Vehicle` might hold a car or a cycle.

The generator also supplies explicit conversion operators. An explicit conversion needs a cast:

```csharp
Vehicle vehicle = new Car(4, 1200);
Car car = (Car)vehicle;

Console.WriteLine(car.CylinderVolumeCc);
```

This prints `1200`. The cast works because this vehicle holds a car. Casting a vehicle that holds a cycle to `Car` throws an exception. The cast does not turn a cycle into a car.

In lunch box terms: "explicit" means we are saying, "I promise there is a sandwich in this box, take it out." If we are right, we get the sandwich. If the box actually holds fruit, the program stops with an error because our promise was wrong.

```text
Vehicle --(cast needed)--> Car     OK only if the box holds a Car
Vehicle --(cast needed)--> Cycle   OK only if the box holds a Cycle
```

That is why a cast is required here but not when putting a value in. Putting a car into a `Vehicle` is always safe. Taking a car out of a `Vehicle` is only safe if we know a car is inside.

When either case is possible, prefer `Match()` or `Switch()` so you handle both.

## Usage: Work with Cars and Cycles Together

Suppose a vehicle shop needs to keep cars and cycles in one list. Using the `Car`, `Cycle`, and `Vehicle` definitions above, we can write:

```csharp
var vehicles = new List<Vehicle>
{
    new Car(4, 1200),
    new Cycle(2, 2)
};

int totalWheels = 0;

foreach (Vehicle vehicle in vehicles)
{
    totalWheels += vehicle.Match(
        car => car.WheelCount,
        cycle => cycle.WheelCount + cycle.TrainingWheelCount
    );

    vehicle.Switch(
        car => Console.WriteLine(
            $"Car: {car.WheelCount} wheels, {car.CylinderVolumeCc} cc"),
        cycle => Console.WriteLine(
            $"Cycle: {cycle.WheelCount} main wheels, {cycle.TrainingWheelCount} training wheels")
    );
}

Console.WriteLine($"Total wheels including training wheels: {totalWheels}");
```

The output is:

```text
Car: 4 wheels, 1200 cc
Cycle: 2 main wheels, 2 training wheels
Total wheels including training wheels: 8
```

Here is how the pieces work together:

- `List<Vehicle>` holds both kinds of vehicle in one collection.
- Implicit conversion wraps each car or cycle in a `Vehicle` when it is added.
- `Match()` returns a wheel count, which we add to the total. This time we include training wheels.
- `Switch()` prints the properties that belong to the actual vehicle type.

We do not need a separate list and loop for each kind. We also do not need to cast each item to guess which properties are available.

## When Is This Way of Writing Useful?

Use this pattern when a value can be one of a small, known set of types, and each type has its own information. It works well for a method parameter, a return value, or a collection that needs to accept those alternatives.

For example, a method taking `Vehicle` can accept a car or a cycle, but not an unrelated type. Inside the method, `Match()` or `Switch()` lets us handle each allowed case with its own properties.

If we add another vehicle type to the union later, calls to `Match()` need a handler for the new case too. This helps us find the places that need updating.

There is no need for OneOf if the program only works with cars: use `Car` directly. If many vehicle types share the same behavior and new types will be added often, an interface or base class may be a better fit.
In more advanced VMs like CPython, the back end may include bytecode generation, bytecode interpretation, and/or JIT compilation, but in a simple interpreter like ARLang, the back end is just AST execution.
