﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using FlowGraphBase.Process;
using FlowGraphBase.Logger;
using FlowGraphBase.Node.StandardEventNode;
using System.ComponentModel;

namespace FlowGraphBase.Node.StandardActionNode
{
    /// <summary>
    /// 
    /// </summary>
    [Visible(false)]
    public class CallFunctionNode
        : ActionNode
    {
        #region Enum

        public enum NodeSlotId : int
        {
            In,
            Out,
            InputStart,
            OutputStart = 1073741823 // int.MaxValue / 2
        }

        #endregion

        #region Fields

        private int m_FunctionID = -1; // used when the node is loaded, in order to retrieve the function
        private SequenceFunction m_Function;

        #endregion //Fields

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public override string Title
        {
            get
            {
                return (GetFunction() == null ? "<null>" : m_Function.Name) + " function";
            }
        }

        #endregion //Properties

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionID_"></param>
        public CallFunctionNode(SequenceFunction function_)
            : base()
        {
            SetFunction(function_);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public CallFunctionNode(XmlNode node_)
            : base(node_)
        {

        }

        #endregion //Constructors

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFunctionSlotChanged(object sender, FunctionSlotChangedEventArg e)
        {
            if (e.Type == FunctionSlotChangedType.Added)
            {
                if (e.FunctionSlot.SlotType == FunctionSlotType.Input)
                {
                    AddFunctionSlot((int)NodeSlotId.InputStart + e.FunctionSlot.ID, SlotType.VarIn, e.FunctionSlot);
                    //AddSlot((int)NodeSlotId.InputStart + e.FunctionSlot.Id, e.FunctionSlot.Name, SlotType.VarIn, typeof(int));
                }
                else if (e.FunctionSlot.SlotType == FunctionSlotType.Output)
                {
                    AddFunctionSlot((int)NodeSlotId.OutputStart + e.FunctionSlot.ID, SlotType.VarOut, e.FunctionSlot);
                    //AddSlot((int)NodeSlotId.OutputStart + e.FunctionSlot.Id, e.FunctionSlot.Name, SlotType.VarOut, typeof(int));
                }
            }
            else if (e.Type == FunctionSlotChangedType.Removed)
            {
                if (e.FunctionSlot.SlotType == FunctionSlotType.Input)
                {
                    RemoveSlotById((int)NodeSlotId.InputStart + e.FunctionSlot.ID);
                }
                else if (e.FunctionSlot.SlotType == FunctionSlotType.Output)
                {
                    RemoveSlotById((int)NodeSlotId.OutputStart + e.FunctionSlot.ID);
                }
            }

            OnPropertyChanged("Slots");
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateNodeSlot()
        {
            GetFunction();

            foreach (SequenceFunctionSlot slot in m_Function.Inputs)
            {
                AddFunctionSlot((int)NodeSlotId.InputStart + slot.ID, SlotType.VarIn, slot);
                //AddSlot((int)NodeSlotId.InputStart + slot.Id, slot.Name, SlotType.VarIn, slot.VarType);
            }

            foreach (SequenceFunctionSlot slot in m_Function.Outputs)
            {
                AddFunctionSlot((int)NodeSlotId.OutputStart + slot.ID, SlotType.VarOut, slot);
                //AddSlot((int)NodeSlotId.OutputStart + slot.Id, slot.Name, SlotType.VarOut, slot.VarType);
            }

            OnPropertyChanged("Slots");
            //OnPropertyChanged("SlotVariableIn");
            //OnPropertyChanged("SlotVariableOut");

//             SlotConnectorIn
//             SlotConnectorOut
//             SlotVariableIn
//             SlotVariableOut
//             SlotVariableInOut
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SequenceFunction GetFunction()
        {
            if (m_Function == null
                && m_FunctionID != -1)
            {
                SetFunction(GraphDataManager.Instance.GetFunctionByID(m_FunctionID));
            }

            return m_Function;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void SetFunction(SequenceFunction func_)
        {
            m_Function = func_;
            m_Function.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnFuntionPropertyChanged);
            m_Function.FunctionSlotChanged += new EventHandler<FunctionSlotChangedEventArg>(OnFunctionSlotChanged);
            UpdateNodeSlot();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "Completed", SlotType.NodeOut);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ProcessingInfo ActivateLogic(ProcessingContext context_, NodeSlot slot_)
        {
            ProcessingInfo info = new ProcessingInfo();
            info.State = ActionNode.LogicState.Ok;
            ActivateOutputLink(context_, (int)NodeSlotId.Out);
            context_.RegisterNextSequence(GetFunction(), typeof(OnEnterFunctionEvent), null);
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override SequenceNode CopyImpl()
        {
            return new CallFunctionNode(m_Function);
        }

        #region Persistence

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        protected override void Load(XmlNode node_)
        {
            base.Load(node_);
            m_FunctionID = int.Parse(node_.Attributes["functionID"].Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionListNode_"></param>
        /// <param name="sequence_"></param>
        internal override void ResolveLinks(XmlNode connectionListNode_, SequenceBase sequence_)
        {
            GetFunction();
            base.ResolveLinks(connectionListNode_, sequence_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public override void Save(XmlNode node_)
        {
            base.Save(node_);
            node_.AddAttribute("functionID", GetFunction().Id.ToString());
        }

        #endregion // Persistence

        #region Link with SequenceFunction

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFuntionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    OnPropertyChanged("Title");
                    break;
            }
        }

        #endregion // Link with SequenceFunction

        #endregion //Methods
    }
}
