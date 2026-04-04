"use client"

import { useState } from "react"
import { ConditionalDialog } from "../ConditionalDialog/ConditionalDialog"
import { Button } from "@chakra-ui/react"
import { ErrorMessage, SuccessMessage } from "@/lib/message"
import { deleteApiOAuthGoogleCalendar } from "@/client"

export default function DisconnectGoogleAccount() {
    const [loading, setLoading] = useState<boolean>(false)

    async function handleSubmit() {
        setLoading(true)
        const res = await deleteApiOAuthGoogleCalendar()
        if (res.data) {
            SuccessMessage("Disconnect Successful", "You google Account has been disconnected successfully")
            setLoading(false)
            window.location.reload()

        } else if (res.error) {
            ErrorMessage(res.error)
        } else {
            ErrorMessage({ title: "Disconnect failed", detail: "An unknown error occurred" })
        }
        setLoading(false)
    }

    return (
        <>
            <ConditionalDialog
                title="Disconnect Google Account"
                detail="Are you sure you want to disconnect the Google Account?"
                onTrue={handleSubmit}
                trueButtonText="Disconnect"
                highlight
                loading={loading}
            >
                <Button color="danger" variant='outline' margin='0.5rem'>Disconnect Account</Button>
            </ConditionalDialog>
        </>

    )
}