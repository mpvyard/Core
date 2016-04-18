﻿#region License
/*
Copyright (c) Quantler B.V., All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/
#endregion

using Quantler;
using Quantler.Interfaces;
using Quantler.Templates;
using System.Linq;

//Use a fixed stop amount to manage new positions
internal class FixedStop : RiskManagementTemplate
{
    #region Public Properties

    //Specify stop amount in pips
    [Parameter(20, 50, 10, "Stop Pips")]
    public int stoppips { get; set; }

    #endregion Public Properties

    #region Public Methods

    // Executed before each trade made
    public override bool IsTradingAllowed()
    {
        //We are always allowed to make a new trade
        return true;
    }

    // Executed when a new order has been created
    public PendingOrder RiskManagement(PendingOrder pendingOrder, AgentState state)
    {
        //Check if this is an exit order, than do nothing
        if (state != AgentState.EntryLong && state != AgentState.EntryShort)
            return null;

        decimal CurrentClose = CurrentBar[pendingOrder.Order.Symbol].Close;

        var pips = pendingOrder.Order.Security.PipSize * stoppips;

        decimal price = pendingOrder.Order.Direction == Direction.Long ?
                                CurrentClose - pips :
                                CurrentClose + pips;

        //Cancel any pending stop orders
        Portfolio.PendingOrders
                    .Where(x => x.Order.AgentId == Agent.AgentId)
                    .Where(x => x.Order.Symbol == pendingOrder.Order.Symbol &&
                            (x.Order.Type == OrderType.Stop || x.Order.Type == OrderType.StopLimit)).Cancel();

        //Return our current stoporder
        return CreateOrder(pendingOrder.Order.Symbol, Direction.Flat, pendingOrder.Order.Quantity, 0, price);
    }

    #endregion Public Methods
}