﻿using System;

namespace TreeStore.Model.Abstractions
{
    public record CreateEntityRequest(string Name, Guid CategoryId);
}