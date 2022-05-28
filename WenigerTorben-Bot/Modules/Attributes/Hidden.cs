using System;

namespace WenigerTorbenBot.Modules.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class Hidden : Attribute
{ }