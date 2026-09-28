import PageShell
    from "../components/PageShell";

import {
    rooms
} from "../data/mockData";

import "./Themes/Plan.css";

const Rooms = () => (
    <PageShell
        title="Rooms"
        description={
            "Review room capacity and campus information used during scheduling."
        }
    >

        <section
            className="panel-card"
        >

            <div
                className={
                    "panel-heading-row"
                }
            >

                <div>

                    <h2>
                        Available rooms
                    </h2>

                    <p>
                        Frontend sample data
                    </p>

                </div>

                <button
                    className={
                        "primary-button"
                    }
                    type="button"
                    onClick={
                        () =>
                            window.alert(
                                "Room creation will be connected to the backend later."
                            )
                    }
                >
                    Add room
                </button>

            </div>

            <div
                className={
                    "table-scroll"
                }
            >

                <table
                    className={
                        "data-table"
                    }
                >

                    <thead>

                    <tr>
                        <th>
                            Room
                        </th>

                        <th>
                            Campus
                        </th>

                        <th>
                            Capacity
                        </th>

                        <th>
                            Availability
                        </th>
                    </tr>

                    </thead>

                    <tbody>

                    {
                        rooms.map(
                            room => (

                                <tr
                                    key={
                                        room.code
                                    }
                                >

                                    <td>
                                        {
                                            room.code
                                        }
                                    </td>

                                    <td>
                                        {
                                            room.campus
                                        }
                                    </td>

                                    <td>
                                        {
                                            room.capacity
                                        }
                                    </td>

                                    <td>

                                            <span
                                                className={
                                                    `status-badge ${
                                                        room.availability ===
                                                        "Available"
                                                            ? "status-ready"
                                                            : "status-locked"
                                                    }`
                                                }
                                            >
                                                {
                                                    room.availability
                                                }
                                            </span>

                                    </td>

                                </tr>

                            )
                        )
                    }

                    </tbody>

                </table>

            </div>

        </section>

    </PageShell>
);

export default Rooms;