'use client'

import { deleteApiUsersMe } from "@/client";
import { ErrorMessage, SuccessMessage } from "@/lib/message";
import { Button } from "@chakra-ui/react";
import { redirect } from "next/navigation";
import { ConditionalDialog } from "../ConditionalDialog/ConditionalDialog";
import { useState } from "react";

export default function DeleteAccountButton() {
    const [loading, setLoading] = useState<boolean>(false)

    async function handleSubmit() {
        setLoading(true)
        const res = await deleteApiUsersMe()
        if (res.data) {
            SuccessMessage("Deletion Successful", "You will be redirected to the Login-Page...")
            setLoading(false)
            redirect("/auth/login")

        } else if (res.error) {
            ErrorMessage(res.error)
        } else {
            ErrorMessage({ title: "Delete failed", detail: "An unknown error occurred" })
        }
        setLoading(false)
    }

    return (
        <>
            <ConditionalDialog
                title="Delete Account"
                detail="Are you sure you want to delete the Account?"
                onTrue={handleSubmit}
                trueButtonText="Delete"
                highlight
                loading={loading}
            >
                <Button color="danger" variant='outline' margin='0.5rem'>Delete Account</Button>
            </ConditionalDialog>
        </>

    )
}