using Godot;
using System;

public class Command {
    public Command(Entity target_) {
        target = target_;
    }

    public Entity target;

    public virtual void Process() {}
}