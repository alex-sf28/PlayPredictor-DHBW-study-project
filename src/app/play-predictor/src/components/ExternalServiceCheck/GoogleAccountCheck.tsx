"use client"

import { ActiveCalendar, getApiOAuthGoogleCalendarCalendarsActive, getApiOAuthGoogleCalendarState } from "@/client";
import { Link } from "@chakra-ui/react";
import { useEffect, useState } from "react"
import AppAlert from "../Alert/AppAlert";

export default function GoogleAccountCheck() {
    const [calendars, setCalendars] = useState<ActiveCalendar[] | undefined>(undefined);
    const [hasGoogle, setHasGoogle] = useState<boolean | undefined>(undefined)

    useEffect(() => {
        (async () => {
            const hasGoogleRes = await getApiOAuthGoogleCalendarState();
            const activeCalendars = await getApiOAuthGoogleCalendarCalendarsActive()
            setHasGoogle(!hasGoogleRes.error || (!activeCalendars.error && activeCalendars.data.length > 0))

            if (activeCalendars.data)
                setCalendars(activeCalendars.data)

        })()
    }, [])

    if (typeof (hasGoogle) != "undefined" && !hasGoogle)
        return <AppAlert
            status="info"
            title="No Google account connected"
            description={<>Please go to the <Link fontWeight="bold" textDecoration="underline" href="/settings/connected-services">settings</Link> to connect your Google account or to upload a calendar.</>} />


    if (calendars && calendars.length == 0)
        return <AppAlert
            status="info"
            title="No calendars activated"
            description={<>Please go to the <Link fontWeight="bold" textDecoration="underline" href="/settings/connected-services">settings</Link> to activate your personal calendars.</>} />

    return null
}