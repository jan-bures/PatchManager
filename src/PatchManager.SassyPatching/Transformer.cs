﻿using System.Linq;
using Antlr4.Runtime.Tree;
using BepInEx.Logging;
using PatchManager.SassyPatching.Nodes;
using SassyPatchGrammar;

namespace PatchManager.SassyPatching;

public class Transformer : sassy_parserBaseVisitor<Node>
{
    private readonly ManualLogSource _logSource;
    public bool Errored;
    public Transformer( ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    private Node Error(Coordinate location, string error)
    {
        _logSource.LogError($"{location.ToString()}: {error}");
        Errored = true;
        return new ErrorNode(location, error);
    }
    
    public override Node VisitPatch(sassy_parser.PatchContext context) =>
        new SassyPatch(context.GetCoordinate(),
            context.children.Select(Visit)
                .ToList());

    public override Node VisitImport_declaration(sassy_parser.Import_declarationContext context) =>
        new Import(context.GetCoordinate(),
            context.imp
                .Text
                .Unescape());

    public override Node VisitVar_decl(sassy_parser.Var_declContext context) =>
        new VariableDeclaration(context.GetCoordinate(),
            context.variable.Text.TrimFirst(),
            Visit(context.val));

    public override Node VisitStage_def(sassy_parser.Stage_defContext context)
    {
        var location = context.GetCoordinate();
        return ulong.TryParse(context.priority.Text,
            out var priority)
            ? new StageDefinition(location,
                context.stage.Text.Unescape(),
                priority)
            : Error(location,
                "stage priority must be an unsigned integer");
    }

    public override Node VisitFunction_def(sassy_parser.Function_defContext context) =>
        new Function(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.children.Select(Visit)
                .ToList());

    public override Node VisitMixin_def(sassy_parser.Mixin_defContext context) =>
        new Mixin(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.children.Select(Visit)
                .ToList());

    public override Node VisitTop_level_conditional(sassy_parser.Top_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitTop_level_else_else(sassy_parser.Top_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.top_level_statement()
                .Select(Visit)
                .ToList());

    public override Node VisitTop_level_else_if(sassy_parser.Top_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSelection_block(sassy_parser.Selection_blockContext context) =>
        new SelectionBlock(context.GetCoordinate(),
            context.attributed_selector()
                .attribute()
                .Select(Visit)
                .Cast<SelectorAttribute>()
                .ToList(),
            Visit(context.attributed_selector()
                .selector()) as Selector,
            context.selector_body()
                .children.Select(Visit)
                .ToList());

    public override Node VisitRequire_mod(sassy_parser.Require_modContext context) =>
        new RequireModAttribute(context.GetCoordinate(), context.guid.Text.Unescape());

    public override Node VisitRequire_not_mod(sassy_parser.Require_not_modContext context) =>
        new RequireNotModAttribute(context.GetCoordinate(), context.guid.Text.Unescape());

    public override Node VisitRun_at_stage(sassy_parser.Run_at_stageContext context) =>
        new RunAtStageAttribute(context.GetCoordinate(), context.stage.Text.Unescape());

    public override Node VisitSel_element(sassy_parser.Sel_elementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    public override Node VisitSel_ruleset(sassy_parser.Sel_rulesetContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());

    public override Node VisitSel_child(sassy_parser.Sel_childContext context)
        => new ChildSelector(context.GetCoordinate(), Visit(context.parent) as Selector,
            Visit(context.child) as Selector);

    public override Node VisitSel_add_element(sassy_parser.Sel_add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText().TrimFirst());

    public override Node VisitSel_class(sassy_parser.Sel_classContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitSel_name(sassy_parser.Sel_nameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitSel_intersection(sassy_parser.Sel_intersectionContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitSel_everything(sassy_parser.Sel_everythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    public override Node VisitSel_without_class(sassy_parser.Sel_without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitSel_without_name(sassy_parser.Sel_without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitSel_combination(sassy_parser.Sel_combinationContext context)
        => new CombinationSelector(context.GetCoordinate(),Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitRuleset_selector(sassy_parser.Ruleset_selectorContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());

    public override Node VisitCombination_selector(sassy_parser.Combination_selectorContext context)
        => new CombinationSelector(context.GetCoordinate(),Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitWithout_name(sassy_parser.Without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());
    
    public override Node VisitClass_selector(sassy_parser.Class_selectorContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitWithout_class(sassy_parser.Without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitName(sassy_parser.NameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitAdd_element(sassy_parser.Add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText().TrimFirst());

    public override Node VisitEverything(sassy_parser.EverythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    public override Node VisitIntersection_selector(sassy_parser.Intersection_selectorContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitElement(sassy_parser.ElementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    public override Node VisitSel_level_conditional(sassy_parser.Sel_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSel_level_else_else(sassy_parser.Sel_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.selector_statement()
                .Select(Visit)
                .ToList());

    public override Node VisitSel_level_else_if(sassy_parser.Sel_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSet_value(sassy_parser.Set_valueContext context)
        => new SetValue(context.GetCoordinate(),Visit(context.expr) as Expression);

    public override Node VisitDelete_value(sassy_parser.Delete_valueContext context)
    {
        return base.VisitDelete_value(context);
    }

    public override Node VisitMerge_value(sassy_parser.Merge_valueContext context)
    {
        return base.VisitMerge_value(context);
    }

    public override Node VisitElement_key_field(sassy_parser.Element_key_fieldContext context)
    {
        return base.VisitElement_key_field(context);
    }

    public override Node VisitString_key_field(sassy_parser.String_key_fieldContext context)
    {
        return base.VisitString_key_field(context);
    }

    public override Node VisitNumber_indexor(sassy_parser.Number_indexorContext context)
    {
        return base.VisitNumber_indexor(context);
    }

    public override Node VisitElement_indexor(sassy_parser.Element_indexorContext context)
    {
        return base.VisitElement_indexor(context);
    }

    public override Node VisitClass_indexor(sassy_parser.Class_indexorContext context)
    {
        return base.VisitClass_indexor(context);
    }

    public override Node VisitString_indexor(sassy_parser.String_indexorContext context)
    {
        return base.VisitString_indexor(context);
    }
}