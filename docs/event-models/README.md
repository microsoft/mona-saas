# Event models

Mona exposes subscription-related events to your downstream handlers (Logic Apps, Functions, etc.) through events published by default to a custom Event Grid topic as highilghted in the diagram below.

![Mona events](../images/mona_arch_overview_events.png)

This doc provides a reference for the various event models that Mona can publish. These models are broken down by version below.

## [Version `2021-10-01`](2021-10-01.md) (Current)

* Flattened event models for simplified downstream consumption
* Switched to pascal-cased, human-readable names

## [Version `2021-05-01`](2021-05-01.md)

* Initial version
