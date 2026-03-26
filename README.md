# CS2 Performance Prediction based on Calendar Data

## Overview

This project is a web application developed as part of a study project. It investigates the relationship between personal calendar data and player performance in **Counter-Strike 2**, using match data from the FACEIT platform.

The main objective is to build a data-driven model that can predict a player's in-game performance at a specific point in time based on contextual factors such as daily schedule, activity patterns, and match history.

## Motivation

Player performance in competitive games is influenced by many external factors, such as time of day, fatigue, or daily routines. This project explores whether calendar data can provide meaningful insights into these factors and improve performance prediction.

## Features

* Integration of FACEIT match data (e.g., kills, deaths, win rate, match timestamps)
* Integration of Google Calendar data (event timing and duration)
* Data analysis and visualization of player activity and performance patterns
* Session-based analysis (e.g., matches played consecutively)
* Machine learning model for performance prediction
* Web-based user interface

## Tech Stack

### Frontend

* Next.js (React-based framework)
* Chakra UI (component library)

### Backend

* ASP.NET Core (C#)
* Entity Framework Core (ORM)

### Database

* PostgreSQL

### External APIs

* FACEIT API (match and player data)
* Google Calendar API (calendar events)

## Data Processing

* Match data and calendar data are combined based on timestamps
* Sessions are identified based on time gaps between matches
* Features are extracted, such as:

  * Matches per session
  * Time of day
  * Day of week
  * Break durations
  * Calendar event density

## Privacy and Data Protection

All collected data is anonymized and used exclusively for research purposes.

* Calendar event content is **not analyzed semantically**
* Event titles and descriptions are stored in an **irreversibly encrypted form**
* Only metadata such as timestamps and durations are used for analysis

## Project Status

This project is currently under development as part of a study project. Features and implementation details may change over time.

## Future Work

* Improvement of the prediction model
* Extension of supported data sources
* Enhanced data visualizations
* Public deployment of the application

## Disclaimer

This project is for academic purposes only and is not affiliated with FACEIT, Valve, or Google.

## Contact
**Mail:** [playpredictor.de@gmail.com](mailto:playpredictor.de@gmail.com)