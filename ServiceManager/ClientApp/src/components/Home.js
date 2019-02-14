import React, { Component } from 'react';
import axios from 'axios';

export class Home extends Component {
    
    constructor (props) {
        super(props);
        this.state = { accessToken: this.props.match.params.accessToken, services: [], loading: true };
        fetch('api/Service/GetServiceData?token=' + this.state.accessToken)
            .then(response => response.json())
            .then(data => {
                this.setState({services : data, files : [], loading: false});
            });
    }
    
    startService(serviceId) {
        fetch('api/Service/RunService?token=' + this.state.accessToken + '&serviceId=' + serviceId)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'success') {
                    let ser = this.state.services;
                    ser[serviceId].state = 'Running';
                    this.setState({services: ser});
                }
            });
    }
    
    stopService(serviceId) {
        fetch('api/Service/StopService?token=' + this.state.accessToken + '&serviceId=' + serviceId)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'success') {
                    let ser = this.state.services;
                    ser[serviceId].state = 'Stopped';
                    this.setState({services: ser});
                }
            });
    }
    
    clearDatabase(serviceId) {
        if (window.confirm('Do you really want to delete database ?')) {
            fetch('api/Service/DeleteDatabase?token=' + this.state.accessToken + '&serviceId=' + serviceId)
                .then(response => response.json())
                .then(data => {
                    if (data.status === 'success') {
                        alert('Service database cleared successfully.');
                    }
                });
        }
    }
    
    handleSelectedFiles(serviceId, event) {
        let cache = [];
        alert('selected files ' + JSON.stringify(event.target, function(key, value) {
            if (typeof value === 'object' && value !== null) {
                if (cache.indexOf(value) !== -1) {
                    try {
                        return JSON.parse(JSON.stringify(value));
                    } catch (error) {
                        return;
                    }
                }
                cache.push(value);
            }
            return value;
        }));
        cache = null;
        
        let sfs = this.state.files;
        sfs[serviceId] = event.target.value;
        this.setState({
            files: sfs,
        });
    }

    handleUpload() {
        for (let counter = 0; counter < this.state.services.length; counter++) {
            let files = this.refs['fileInput' + counter].files;
            if (files !== undefined && files.length > 0) {
                let index = counter;
                const data = new FormData();
                data.append('ServiceId', index.toString());
                data.append('Token', this.state.accessToken);
                Array.prototype.forEach.call(files, file => {
                    data.append('Files', file);
                });
                axios.post("api/Service/UpdateService", data)
                    .then(res => {
                        if (res.data.status === 'success') {
                            alert('success : ' + this.state.services[index].name + ' updated.');
                        } else {
                            alert('error : ' + this.state.services[index].name + ' not updated.');
                        }
                    });
            }
        }
    }

    renderServicesTable (serviceDataList) {
        return (
            <table className='table table-striped'>
                <thead>
                <tr>
                    <th> ServiceID</th>
                    <th>Service</th>
                    <th>Status</th>
                    <th>Life Control</th>
                    <th>Version Control</th>
                    <th>Database</th>
                </tr>
                </thead>
                <tbody>
                {serviceDataList.map(serviceData =>
                    <tr key={serviceData.id}>
                        <td>{serviceData.id}</td>
                        <td>{serviceData.name}</td>
                        <td>{serviceData.state}</td>
                        <td>
                            <button onClick={() => {serviceData.state === 'Running' ? 
                                this.stopService(serviceData.id) : this.startService(serviceData.id)}} 
                                    style={{width:'72px', height: '28px'}}>
                                {serviceData.state === 'Running' ? 'Stop' : 'Run'}
                            </button>
                        </td>
                        <td>
                            <input id='Files' name={'Files'} type='file' ref={'fileInput' + serviceData.id} multiple/>
                        </td>
                        <td>
                            <button onClick={() => this.clearDatabase(serviceData.id)}>
                                Clear Data
                            </button>
                        </td>
                    </tr>
                )}
                <tr key={serviceDataList.length}>
                    <td>{}</td>
                    <td>{}</td>
                    <td>{}</td>
                    <td>{}</td>
                    <td>
                        <button onClick={() => {this.handleUpload()}}>Upload Files</button>
                    </td>
                    <td>{}</td>
                </tr>
                </tbody>
            </table>
        );
    }

    render () {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderServicesTable(this.state.services);

        return (
            <div>
                <h1>Services Data</h1>
                <p>This is services control panel located on main server</p>
                {contents}
            </div>
        );
    }
}
