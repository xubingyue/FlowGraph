﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulator.Undo
{
    /// <summary>
    /// An undo Command
    /// </summary>
    public interface IUndoCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1_"></param>
        void Redo();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1_"></param>
        void Undo();
    }
}
