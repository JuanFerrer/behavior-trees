﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBehaviorTree
{
    public abstract class Node
    {
        public string Name { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsOpen { get; private set; }
        public Status Result { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Status Tick()
        {
            Enter();

            if (!IsOpen) Open();

            var status = tick();

            Exit();
            if (status == Status.RUNNING) return status;

            Close();
            return status;
        }

        /// <summary>
        /// Enter node and prepare for execution
        /// </summary>
        private void Enter()
        {
            //
        }

        /// <summary>
        /// Open node only if node has not been opened before
        /// </summary>
        private void Open()
        {
            IsOpen = true;
            IsClosed = false;
        }

        /// <summary>
        /// Actual tick to be overriden by every child
        /// </summary>
        protected abstract Status tick();

        /// <summary>
        /// Exit node after every tick
        /// </summary>
        private void Exit()
        {
            //
        }

        /// <summary>
        /// Close node to ensure we don't go through it again
        /// </summary>
        private void Close()
        {
            IsOpen = false;
            IsClosed = true;
        }

    }
}
