import { RouterProvider, createBrowserRouter} from 'react-router-dom'
import { Index } from "./Pages/Index";
import 'react-photo-view/dist/react-photo-view.css';

const routers = createBrowserRouter([
    {
        path: "/",
        element: <Index/>
    }
])

export function App() {
    return <>
       <RouterProvider router={routers} />
    </>
}
