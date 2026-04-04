import { Button } from "@chakra-ui/react";
import { getApiAuthGoogleCalendar } from "@/client";
import { ErrorMessage } from "@/lib/message";

export default function ConnectGoogleCalendar() {

    async function connectCalendar() {
        const res = await getApiAuthGoogleCalendar({ query: { redirectUri: window.location.href } })

        if (res.data) {
            window.location.href = res.data
        } else if (res.error) {
            ErrorMessage(res.error)
        }

    }

    return <Button colorPalette="brand" onClick={connectCalendar}>Connect Google Calendar</Button>
}