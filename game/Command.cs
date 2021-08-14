using Godot;
using System;

public class Command<T> where T : Entity {
    public Command(T target_) {
        target = target_;
    }

    public T target;

    public virtual void Execute() {}
}
