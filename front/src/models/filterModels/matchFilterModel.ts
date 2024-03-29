﻿import {Hero} from "../Hero";

export class MatchFilterModel {
    minDurationInMinutes?: number | undefined = null;
    maxDurationInMinutes?: number | undefined = null;
    minStartedMillisecondsBefore?: number | undefined;
    maxStartedMillisecondsBefore?: number | undefined;

    selfTeam: Hero[] = [];
    otherTeam: Hero[] = [];

    skip?: number | undefined;
    take?: number | undefined;
    daysAgo?: number | undefined;
}