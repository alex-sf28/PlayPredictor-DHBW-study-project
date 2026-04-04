import { Avatar, Box, LinkBox, LinkOverlay } from "@chakra-ui/react";
import { FaChildCombatant } from "react-icons/fa6";
import { PiChartDonut, PiChartLineUp } from "react-icons/pi";
import NavigationTabs from "./NavigationTabs/NavigationTabs";
import { navBarFields } from "@/config/navBarFields";
import OptionMenu from "../OptionMenu/OptionMenu";
import { menuOptionsLoggedIn } from "@/config/userIconMenuOptions";
import UserProfileCircle from "./UserProfileCircle/UserProfileCircle";

export default function Navbar() {

    return (
        <Box
            width="100%"
            display="flex"
            flexDirection="row"
            borderYWidth="1px"

            minHeight="4rem"
            alignItems="stretch"
        >
            <LinkBox display="flex" flexDirection="row" alignItems="center" marginX="1rem">
                <FaChildCombatant size="2rem" />
                <LinkOverlay href="/" />
            </LinkBox>

            <NavigationTabs navBarFields={navBarFields} />

            <Box alignSelf="center" marginLeft="auto" paddingX="0.7rem">
                <UserProfileCircle />
            </Box>
        </Box>
    );
}
