# ![AReSSO Logo](aresso_icon.svg) AReSSO

**A**ction > **Re**ducer > **S**tore > **S**elector > **O**bserver

AReSSO is a unidirectional state container (a la Redux) intended for use in Unity 3D. It is definitely WIP at the moment, however the basic Redux-like functionality is there.

## Dependencies

AReSSO depends on UniRx by [neuecc](https://github.com/neuecc) (<https://github.com/neuecc/UniRx>). I recommend using the UPM-compatible fork by [starikcetin](https://github.com/starikcetin) (<https://github.com/starikcetin/UniRx>).

## Usage

For more extensive usage examples, visit [the example projects repo](https://github.com/schultzcole/AReSSO-Example-Projects).

The `Store` class is the center of AReSSO. It currently has the following functionality:

### Creation

When creating a `Store`, you must specify the type of the root of your state hierarchy, an initial value for your state hierarchy, and a root reducer:

``` csharp
MyStateRoot RootReducer(MyStateRoot state, IAction action)
{
    // perform some transformation
    return newState;
}

// ...

MyStateRoot initialState = new MyStateRoot( /* state initialization params */ );
Store<MyStateRoot> myStore = new Store<MyStateRoot>(initialState, RootReducer);
```

### Dispatching

After your store has been created, you can dispatch actions to it using `Store.Dispatch`:

``` csharp
myStore.Dispatch(new MyAction( /* action params */ ));
```

`Dispatch` will subsequently pass the action to the root reducer provided at creation, set the new state, and notify observers.

### Getting the State

You can use the `Store.State` property to get the current state at any time. You can also get a particular segment of the state using `Select`, which allows you to pass a selector function which will get the particular piece of state you are interested in from the root state object:

``` csharp
var justTheName = myStore.Select(state => state.Person.Name);
```

 You can also get an `IObservable` of which will emit changes to the state. This is done using `State.ObservableFor`, which also takes a selector function, similar to `Store.State`:

``` csharp
myStore.ObservableFor(state => state.Person.Name).Subscribe(name => Debug.Log($"Name changed to {name}!"))
```

This example uses reactive extensions (UniRx) to subscribe to the resulting IObservable with a lambda.

`ObservableFor` has an additional optional parameter, `notifyImmediately`. If this parameter is `true`, the returned `IObservable` will emit the current state as soon as it is subscribed. If it is `false`, the observable will emit the next time that segment of state has changed.

## Future Feature Additions

- Add useful example projects. These will likely be in a different repo. (issue #4)
- Add SideEffectors. SideEffectors are intended to be AReSSO's corollary to Redux's Middlewares or ngrx's Effects. They react to actions to accomplish side effects, whereas reducers cannot accomplish side effects. (issue #3)
- AReSSO devtools. I want to add Redux-like devtools in the Unity Editor to allow for time-travel debugging, etc. (issue #5)
