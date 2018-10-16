﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBehaviorTree
{
    public class Repeater : Decorator
    {
        private int n;

        public Repeater(string name, int times = 0)
        {
            this.Name = name;
            n = times;
        }

        public override Node Copy()
        {
            Repeater newNode = new Repeater(this.Name, this.n);
            newNode.AddChild(this.child.Copy());
            return newNode;
        }

        /// <summary>
        /// Repeat n times and return
        /// </summary>
        /// <returns></returns>
        protected override Status tick()
        {
            if (n == 0)
            {
                while (true)
                {
                    this.Result = child.Tick();

                    if (this.Result == Status.ERROR) return this.Result;
                }
            }
            else
            {
                for (int i = 0; i < n; ++i)
                {
                    do
                    {
                        this.Result = child.Tick();
                        if (this.Result == Status.ERROR) return this.Result;
                    } while (child.IsOpen);
                }
            }
            this.Result = Status.SUCCESS;
            return this.Result;
        }
    }
}
