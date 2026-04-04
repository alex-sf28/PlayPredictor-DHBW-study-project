import { NavTabField } from "@/types/navigation";
import { PiChartDonut, PiChartLineUp } from "react-icons/pi";

const navBarFields: NavTabField[] = [
    {
        displayName: <>Analytics <PiChartDonut /></>,
        pathName: "analytics"
    },
    {
        displayName: <>Prognosis <PiChartLineUp /></>,
        pathName: "prognosis"
    }
];

export { navBarFields };