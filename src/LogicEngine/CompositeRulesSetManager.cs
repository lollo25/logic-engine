﻿using LogicEngine.Interfaces;
using LogicEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TinyFp;
using TinyFp.Extensions;

namespace LogicEngine;

public class CompositeRulesSetManager<T, TKey> : ICompositeRulesSetManager<T, TKey> where T : new()
{
    private readonly IRulesSetCompiler _compiler;
    private Func<T, Option<TKey>> _func;

    public CompositeRulesSetManager(IRulesSetCompiler compiler) => _compiler = compiler;

    public IEnumerable<(TKey, RulesSet)> Set 
    {
        set => _func = 
            value
                .ToOption(v => !v.Any())
                .Map(v => v.Where(kv => kv.Item1 is not null))
                .Map<Func<T, Option<TKey>>>(v => (item) => v.Select(t => (t.Item1, _compiler.Compile<T>(t.Item2)))
                .Select(t => (t.Item1, t.Item2.Executables))
                .FirstOrDefault(t => t.Executables.Any(f => f(item).IsRight))
                .ToOption(t => t.Item1 is null)
                .Map(t => t.Item1))
                .OrElse(() => item => Option<TKey>.None());
    }

    public Option<TKey> FirstMatching(T item) => _func(item);
}