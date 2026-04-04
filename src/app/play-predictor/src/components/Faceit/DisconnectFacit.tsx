import { ErrorMessage, SuccessMessage } from "@/lib/message"
import { useState } from "react"
import { ConditionalDialog } from "../ConditionalDialog/ConditionalDialog"
import { Button } from "@chakra-ui/react"
import { deleteApiOAuthFaceitPlayers } from "@/client"

export default function DisconnectFaceitAccount() {
    const [loading, setLoading] = useState<boolean>(false)

    async function handleSubmit() {
        setLoading(true)
        const res = await deleteApiOAuthFaceitPlayers()
        if (res.data) {
            SuccessMessage("Disconnect Successful", "Your Faceit account has been disconnected successfully")
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
                title="Disconnect Faceit Account"
                detail="Are you sure you want to disconnect the Faceit Account?"
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