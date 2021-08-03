﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;

namespace Blazr.SPA.Brokers
{
    public class DateTimeBroker :
        IDateTimeBroker
    {
        public DateTimeOffset GetCurrentDateTime()
            => DateTimeOffset.UtcNow;
    }
}