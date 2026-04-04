"use client"

import { getApiOAuthFaceitPlayers, Player } from "@/client";
import OptionMenu from "@/components/OptionMenu/OptionMenu";
import { menuOptionsLoggedIn, menuOptionsLoggedOut } from "@/config/userIconMenuOptions";
import { useAuth } from "@/lib/authStore";
import { Avatar, SkeletonCircle } from "@chakra-ui/react";
import { useEffect, useState } from "react";


export default function UserProfileCircle() {

    const { user, accessToken, isLoading } = useAuth()
    const [faceitAccount, setFaceitAccount] = useState<Player>()
    const [loading, setLoading] = useState<boolean>(true)

    const isLoggedIn = () => (user != null && accessToken != null)

    useEffect(() => {
        if (!isLoading) {
            (async () => {
                const res = await getApiOAuthFaceitPlayers();
                if (res.data)
                    setFaceitAccount(res.data)

                setLoading(false)
            })()
        }
    }, [user, isLoading])

    return (
        <SkeletonCircle loading={isLoading || loading}>
            <OptionMenu itemList={(isLoggedIn()) ? menuOptionsLoggedIn : menuOptionsLoggedOut}>

                <Avatar.Root
                    cursor="pointer"
                    colorPalette={isLoggedIn() ? "brand" : undefined}
                >
                    {faceitAccount?.avatar != null ? <Avatar.Image src={faceitAccount?.avatar ?? null} /> : null}
                    <Avatar.Fallback />
                </Avatar.Root>

            </OptionMenu>
        </SkeletonCircle>
    )
} 