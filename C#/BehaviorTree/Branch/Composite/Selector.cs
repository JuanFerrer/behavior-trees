﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBehaviorTree
{
    public class Selector : Composite
    {
        public Selector(string name)
        {
            this.Name = name;
            children = new List<Node>();
        }

        public override object Clone()
        {
            Selector newNode = new Selector(this.Name);
            for (int i = 0, size = children.Count; i < size; ++i)
            {
                newNode.AddChild(children[i].Clone() as Node);
            }
            return newNode;
        }

        /// <summary>
        /// Propagate tick to children. Return FAILURE if no child succeeds
        /// </summary>
        /// <returns></returns>
        protected override Status tick()
        {
            if (this.Result == Status.RUNNING)
            {
                foreach (var n in children)
                {
                    if (!n.IsOpen && n.Result == Status.SUCCESS)
                    {
                        this.Result = Status.SUCCESS;
                        return this.Result;
                    }
                    else if (n.Result == Status.RUNNING)
                    {
                        this.Result = n.Tick();
                        if (this.Result != Status.FAILURE) return this.Result;
                    }
                }
                this.Result = Status.FAILURE;
            }
            return this.Result;
        }
    }
}
