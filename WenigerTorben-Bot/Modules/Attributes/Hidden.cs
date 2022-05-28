using System;

namespace WenigerTorbenBot.Modules.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class Hidden : Attribute
{}